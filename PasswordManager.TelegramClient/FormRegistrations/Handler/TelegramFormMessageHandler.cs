using Microsoft.EntityFrameworkCore;
using PasswordManager.TelegramClient.Data;
using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.Messenger;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.FormRegistrations.Handler;

public class TelegramFormMessageHandler
{
    private readonly Type _defaultForm = typeof(MainMenu);

    private readonly IDbContextFactory<TelegramClientContext> _contextFactory;
    
    private readonly IUserDataRepository _userDataRepository;
    
    private readonly IMessengerClient _client;

    private readonly Dictionary<string, FormModel> _formModels = new();

    public TelegramFormMessageHandler(IDbContextFactory<TelegramClientContext> contextFactory, IEnumerable<IFormRegistration> formRegistrations, 
        IUserDataRepository userDataRepository, IMessengerClient client)
    {
        _contextFactory = contextFactory;
        _userDataRepository = userDataRepository;
        _client = client;

        foreach (var formRegistration in formRegistrations)
        {
            var formModel = formRegistration.ResolveForm();
            _formModels.Add(formRegistration.GetType().FullName!, formModel);
        }
    }
    
    public Task<Type> GetFormByMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        var formType = _defaultForm;
        var form = _formModels.FirstOrDefault(x => x.Value.Commands.Contains(message.Text));
        if (Type.GetType(form.Key ?? string.Empty) is not null and var type) formType = type;
        return Task.FromResult(formType);
    }

    public async Task<Type?> GetActiveFormAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return Type.GetType((await GetOneAsync(telegramUserId, context, true, cancellationToken))?.FormType ?? string.Empty);
    }
    
    public async Task StartFormRequestAsync<TForm>(long userId, long chatId, CancellationToken cancellationToken = default)
        where TForm: IFormRegistration
    {
        await StartFormRequestAsync(typeof(TForm), userId, chatId, cancellationToken);
    }

    public async Task StartFormRequestAsync(Type formType, long userId, long chatId, CancellationToken cancellationToken = default)
    {
        await FinishCurrentFormAsync(userId, cancellationToken);
        
        var formEntity = new TelegramUserRequestFormEntity()
        {
            UserId = userId,
            FormType = formType.FullName!,
            CurrentStep = 0
        };
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.RequestForms.AddAsync(formEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await context.Entry(formEntity).Reference(x => x.User).LoadAsync(cancellationToken);

        await WriteQuestionAsync(_client, chatId, formEntity, cancellationToken);
        await HandleFormRequestAsync(_client, new Message()
        {
            From = new User()
            {
                Id = userId,
            },
            Chat = new Chat()
            {
                Id = chatId
            }
        }, cancellationToken);
    }

    public async Task HandleFormRequestAsync(IMessengerClient client, Message message, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var formEntity = await GetOneAsync(message.From!.Id, context, false, cancellationToken);
        if (formEntity is null) return;

        var userData = await _userDataRepository.GetUserDataAsync(message.From.Id, cancellationToken);
        
        var currentForm = _formModels[formEntity.FormType];

        if (currentForm.Steps.Count > formEntity.CurrentStep)
        {
            var currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep)
                                               .BuildAsync(userData, formEntity.Data!, this, cancellationToken);

            if (currentStep.NextForms.TryGetValue(message.Text ?? string.Empty, out var nextForm))
            {
                await StartFormRequestAsync(nextForm, message.From.Id, message.Chat.Id, cancellationToken);

                return;
            }

            if (currentStep.IsDeleteAnswer)
                await client.DeleteMessageAsync(message.MessageId, message.Chat.Id, cancellationToken: cancellationToken);

            var validateResult = currentStep.Validator?.Invoke(new ValidateAnswerEventArgs()
            {
                Answer = message.Text!,
                UserData = userData,
                Context = formEntity.Data
            }, cancellationToken);

            if (validateResult is { IsSuccess: false })
            {
                await client.SendMessageAsync(validateResult.Error!, message.Chat.Id,
                    cancellationToken: cancellationToken);

                return;
            }

            formEntity.Data ??= new();
            formEntity.Data = new(formEntity.Data);
            formEntity.Data.Add(currentStep.AnswerKey, validateResult?.ValidResult ?? message.Text!);

            formEntity.CurrentStep++;
        }

        if (formEntity.CurrentStep >= currentForm.Steps.Count)
        {
            await currentForm.OnComplete(new OnCompleteFormEventArgs()
            {
                Answers = formEntity.Data,
                UserData = userData,
                Client = client,
                ChatId = message.Chat.Id,
                FormMessageHandler = this
            }, cancellationToken);
            await FinishCurrentFormAsync(message.From.Id, cancellationToken);
        }
        else
        {
            await WriteQuestionAsync(client, message.Chat.Id, formEntity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task FinishCurrentFormAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var currentForm = await context.RequestForms.FirstOrDefaultAsync(x => x.UserId.Equals(telegramUserId), cancellationToken);
        if (currentForm is null) return;
        
        context.RequestForms.Remove(currentForm);
        await context.SaveChangesAsync(cancellationToken);
    }
    
    private async Task WriteQuestionAsync(IMessengerClient client, long chatId, TelegramUserRequestFormEntity formEntity, CancellationToken cancellationToken = default)
    {
        var currentForm = _formModels[formEntity.FormType];
        
        if (formEntity.CurrentStep >= currentForm.Steps.Count)
            return;
        
        var currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep).BuildAsync(formEntity.User, formEntity.Data!, this, cancellationToken);

        if (currentStep.Question is null)
            return;

        var questionMessage = await client.SendMessageAsync(currentStep.Question, chatId, currentStep.Answers, 
            disableWebPagePreview: currentStep.IsDisableWebPagePreview, cancellationToken: cancellationToken);
        if (currentStep.TimeBeforeQuestionDeletion.HasValue)
            _ = DeleteMessageAfterDelayAsync(client, chatId, questionMessage.MessageId,
                currentStep.TimeBeforeQuestionDeletion.Value, cancellationToken: cancellationToken);
    }
    
    private async Task<TelegramUserRequestFormEntity?> GetOneAsync(long telegramUserId, TelegramClientContext context, 
        bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        return await (asNoTracking ? context.RequestForms.AsNoTracking() : context.RequestForms)
                     .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.TelegramUserId.Equals(telegramUserId), cancellationToken);
    }
    
    private async Task DeleteMessageAfterDelayAsync(IMessengerClient client, long chatId, int messageId, TimeSpan delay, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken);
        await client.DeleteMessageAsync(messageId, chatId, cancellationToken: cancellationToken);
    }
}

public interface IFormRegistration
{
    FormModel ResolveForm();
}