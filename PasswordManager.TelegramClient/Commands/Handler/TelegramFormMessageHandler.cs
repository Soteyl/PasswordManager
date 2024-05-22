using Microsoft.EntityFrameworkCore;
using PasswordManager.TelegramClient.Data;
using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class TelegramFormMessageHandler
{
    private readonly TelegramClientContext _context;
    
    private readonly IUserDataRepository _userDataRepository;
    
    private readonly IMessengerClient _client;

    private readonly Dictionary<string, FormModel> _formModels = new();

    public TelegramFormMessageHandler(TelegramClientContext context, IEnumerable<IFormRegistration> formRegistrations, 
        IUserDataRepository userDataRepository, IMessengerClient client)
    {
        _context = context;
        _userDataRepository = userDataRepository;
        _client = client;

        foreach (var formRegistration in formRegistrations)
        {
            var formModel = formRegistration.ResolveForm();
            _formModels.Add(formRegistration.GetType().FullName!, formModel);
        }
    }

    public async Task<bool> HasActiveFormAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        return await GetOneAsync(telegramUserId, true, cancellationToken) is not null;
    }
    
    public async Task StartFormRequestAsync<TForm>(long userId, long chatId, CancellationToken cancellationToken = default)
        where TForm: IFormRegistration
    {
        await StartFormRequestAsync(typeof(TForm), userId, chatId, cancellationToken);
    }

    public async Task StartFormRequestAsync(Type formType, long userId, long chatId, CancellationToken cancellationToken = default)
    {
        
        var formEntity = new TelegramUserRequestFormEntity()
        {
            UserId = userId,
            FormType = formType.FullName!,
            CurrentStep = 0
        };
        await _context.RequestForms.AddAsync(formEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _context.Entry(formEntity).Reference(x => x.User).LoadAsync(cancellationToken);

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
        var formEntity = await GetOneAsync(message.From!.Id, false, cancellationToken);
        if (formEntity is null) return;

        var userData = await _userDataRepository.GetUserDataAsync(message.From.Id, cancellationToken);
        
        var currentForm = _formModels[formEntity.FormType];
        var currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep).BuildAsync(userData, formEntity.Data!, cancellationToken);

        if (currentStep.NextForms.TryGetValue(message.Text!, out var nextForm))
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
        formEntity.Data.Add(currentStep.AnswerKey, validateResult?.ValidResult ?? message.Text!);
        
        formEntity.CurrentStep++;
        if (formEntity.CurrentStep >= currentForm.Steps.Count)
        {
            await currentForm.OnComplete(new OnCompleteFormEventArgs()
            {
                Answers = formEntity.Data,
                UserData = userData,
                Client = client,
                ChatId = message.Chat.Id
            }, cancellationToken);
            _context.RequestForms.Remove(formEntity);
        }
        else
        {
            await WriteQuestionAsync(client, message.Chat.Id, formEntity, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task FinishCurrentFormAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        var currentForm = await _context.RequestForms.FirstOrDefaultAsync(x => x.UserId.Equals(telegramUserId), cancellationToken);
        if (currentForm is null) return;
        
        _context.RequestForms.Remove(currentForm);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private async Task WriteQuestionAsync(IMessengerClient client, long chatId, TelegramUserRequestFormEntity formEntity, CancellationToken cancellationToken = default)
    {
        var currentForm = _formModels[formEntity.FormType];
        var currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep).BuildAsync(formEntity.User, formEntity.Data!, cancellationToken);

        var questionMessage = await client.SendMessageAsync(currentStep.Question, chatId, currentStep.Answers, cancellationToken: cancellationToken);
        if (currentStep.TimeBeforeQuestionDeletion.HasValue)
            _ = DeleteMessageAfterDelayAsync(client, chatId, questionMessage.MessageId,
                currentStep.TimeBeforeQuestionDeletion.Value, cancellationToken: cancellationToken);
    }
    
    private async Task<TelegramUserRequestFormEntity?> GetOneAsync(long telegramUserId, bool asNoTracking = false, 
        CancellationToken cancellationToken = default)
    {
        return await (asNoTracking ? _context.RequestForms.AsNoTracking() : _context.RequestForms)
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