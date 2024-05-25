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
    private readonly Type? _defaultForm = null;

    private readonly IDbContextFactory<TelegramClientContext> _contextFactory;
    
    private readonly IUserDataRepository _userDataRepository;
    
    private readonly IMessengerClient _client;

    private readonly Dictionary<string, IFormRegistration> _formModels = new();

    public TelegramFormMessageHandler(IDbContextFactory<TelegramClientContext> contextFactory, IEnumerable<IFormRegistration> formRegistrations, 
        IUserDataRepository userDataRepository, IMessengerClient client)
    {
        _contextFactory = contextFactory;
        _userDataRepository = userDataRepository;
        _client = client;

        foreach (var formRegistration in formRegistrations)
        {
            _formModels.Add(formRegistration.GetType().FullName!, formRegistration);
        }
    }
    
    public Task<Type?> GetFormByMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        var formType = _defaultForm;
        var form = _formModels.FirstOrDefault(x => x.Value.ResolveForm().Commands.Contains(message.Text));
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

        var formStepData = new FormStepData()
        {
            ChatId = chatId,
            Message = null,
            MessageId = null,
            UserId = userId
        };
        await WriteQuestionAsync(formStepData, cancellationToken);
    }

    public async Task HandleFormRequestAsync(FormStepData formStepData, CancellationToken cancellationToken = default)
    {
        if (await HandleAnswerAsync(formStepData, cancellationToken))
            if (await MoveToNextStepAsync(formStepData, cancellationToken))
                await WriteQuestionAsync(formStepData, cancellationToken);
    }

    public async Task FinishCurrentFormAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var currentForm = await context.RequestForms.FirstOrDefaultAsync(x => x.UserId.Equals(telegramUserId), cancellationToken);
        if (currentForm is null) return;
        
        context.RequestForms.Remove(currentForm);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> HandleAnswerAsync(FormStepData formStepData, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var formEntity = await GetOneAsync(formStepData.UserId, context, false, cancellationToken);
        if (formEntity is null) return true;
        var currentForm = _formModels[formEntity.FormType].ResolveForm();
        if (currentForm.Steps.Count <= formEntity.CurrentStep) return true;
        var userData = await _userDataRepository.GetUserDataAsync(formStepData.UserId, cancellationToken);
        
        var currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep)
                                           .BuildAsync(userData, formEntity.Data!, this, cancellationToken);

        if (currentStep.IsDeleteAnswer)
            await _client.DeleteMessageAsync(formStepData.MessageId!.Value, formStepData.ChatId, cancellationToken: cancellationToken);

        var validateResult = currentStep.Validator?.Invoke(new ValidateAnswerEventArgs()
        {
            Answer = formStepData.Message!,
            UserData = userData,
            Context = formEntity.Data
        }, cancellationToken);

        if (validateResult is { IsSuccess: false })
        {
            await _client.SendMessageAsync(validateResult.Error!, formStepData.ChatId,
                answers: currentStep.Answers, cancellationToken: cancellationToken);

            return false;
        }
        
        if (!string.IsNullOrEmpty(currentStep.AnswerKey))
        {
            formEntity.Data ??= new();
            formEntity.Data = new(formEntity.Data);
            formEntity.Data.TryAdd(currentStep.AnswerKey, validateResult?.ValidResult ?? formStepData.Message!);
        }

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> MoveToNextStepAsync(FormStepData formStepData, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var formEntity = await GetOneAsync(formStepData.UserId, context, false, cancellationToken);
        var currentForm = _formModels[formEntity!.FormType].ResolveForm();
        var completeArgs = new OnCompleteFormEventArgs()
        {
            Data = formEntity.Data!,
            UserData = formEntity.User,
            Client = _client,
            ChatId = formStepData.ChatId,
            FormMessageHandler = this
        };
        
        if (formEntity.CurrentStep >= currentForm.Steps.Count)
        {
            if (currentForm.OnComplete is not null)
                await currentForm.OnComplete(completeArgs, cancellationToken);
            await FinishCurrentFormAsync(formStepData.UserId, cancellationToken);
            return false;
        }
        
        FormStep? currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep)
                                                 .BuildAsync(formEntity.User, formEntity.Data!, this, cancellationToken);
        if (currentStep.NextForms.TryGetValue(formStepData.Message ?? string.Empty, out var nextForm))
        {
            if (currentForm.OnComplete is not null)
                await currentForm.OnComplete(completeArgs, cancellationToken);
            await StartFormRequestAsync(nextForm, formStepData.UserId, formStepData.ChatId, cancellationToken);
            return false;
        }

        do
        {
            formEntity.CurrentStep++;

            if (formEntity.CurrentStep >= currentForm.Steps.Count)
                break;

            currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep)
                                           .BuildAsync(formEntity.User, formEntity.Data!, this, cancellationToken);
        } while (currentStep?.ShouldBeExecuted() is not true);

        if (formEntity.CurrentStep >= currentForm.Steps.Count)
        {
            if (currentForm.OnComplete is not null)
                await currentForm.OnComplete.Invoke(completeArgs, cancellationToken);
            await FinishCurrentFormAsync(formStepData.UserId, cancellationToken);
            return false;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    private async Task WriteQuestionAsync(FormStepData formStepData, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var formEntity = await GetOneAsync(formStepData.UserId, context, false, cancellationToken);
        var currentForm = _formModels[formEntity.FormType].ResolveForm();
        if (formEntity.CurrentStep >= currentForm.Steps.Count) return;
        
        var currentStep = await currentForm.Steps.ElementAt(formEntity.CurrentStep).BuildAsync(formEntity.User, formEntity.Data!, this, cancellationToken);
        if (currentStep.Question is null) return;

        var questionMessage = await _client.SendMessageAsync(currentStep.Question, formStepData.ChatId, currentStep.Answers, 
            disableWebPagePreview: currentStep.IsDisableWebPagePreview, cancellationToken: cancellationToken);
        if (currentStep.TimeBeforeQuestionDeletion.HasValue)
            _ = DeleteMessageAfterDelayAsync(formStepData.ChatId, questionMessage.MessageId,
                currentStep.TimeBeforeQuestionDeletion.Value, cancellationToken: cancellationToken);
    }
    
    private async Task<TelegramUserRequestFormEntity?> GetOneAsync(long telegramUserId, TelegramClientContext context, 
        bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        return await (asNoTracking ? context.RequestForms.AsNoTracking() : context.RequestForms)
                     .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.TelegramUserId.Equals(telegramUserId), cancellationToken);
    }
    
    private async Task DeleteMessageAfterDelayAsync(long chatId, int messageId, TimeSpan delay, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken);
        await _client.DeleteMessageAsync(messageId, chatId, cancellationToken: cancellationToken);
    }
}

public interface IFormRegistration
{
    FormModel ResolveForm();
}