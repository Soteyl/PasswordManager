using Microsoft.EntityFrameworkCore;
using PasswordManager.TelegramClient.Data;
using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Keyboard;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class TelegramFormMessageHandler
{
    private readonly TelegramClientContext _context;
    
    private readonly IUserDataRepository _userDataRepository;

    private readonly Dictionary<string, FormModel> _formModels = new();

    public TelegramFormMessageHandler(TelegramClientContext context, IEnumerable<IFormRegistration> formRegistrations, IUserDataRepository userDataRepository)
    {
        _context = context;
        _userDataRepository = userDataRepository;

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
    
    public async Task StartFormRequestAsync<TForm>(ITelegramBotClient client, long userId, long chatId, CancellationToken cancellationToken = default)
        where TForm: IFormRegistration
    {
        var formType = typeof(TForm).FullName!;
        var formEntity = new TelegramUserRequestFormEntity()
        {
            UserId = userId,
            FormType = formType,
            CurrentStep = 0
        };
        await _context.RequestForms.AddAsync(formEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteQuestionAsync(client, chatId, formEntity, cancellationToken);
        await HandleFormRequestAsync(client, new Message()
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

    public async Task HandleFormRequestAsync(ITelegramBotClient client, Message message, CancellationToken cancellationToken = default)
    {
        var formEntity = await GetOneAsync(message.From!.Id, false, cancellationToken);
        if (formEntity is null) return;

        var userData = await _userDataRepository.GetUserDataAsync(message.From.Id, cancellationToken);
        
        var currentForm = _formModels[formEntity.FormType];
        var currentStep = currentForm.Steps.ElementAt(formEntity.CurrentStep);

        if (!currentStep.IsWithoutAnswer)
        {
            if (currentStep.IsDeleteAnswer) 
                await client.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken: cancellationToken);
            var validateResult = currentStep.Validator?.Invoke(new ValidateAnswerEventArgs()
            {
                Answer = message.Text!,
                UserData = userData
            }, cancellationToken);
            if (validateResult is { IsSuccess: false })
            {
                await client.SendTextMessageAsync(message.Chat.Id, validateResult.Error!, cancellationToken: cancellationToken);
                return;
            }
            formEntity.Data ??= new();
            formEntity.Data.Add(currentStep.AnswerKey, validateResult!.ValidResult!);
        }
        
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
    
    private async Task WriteQuestionAsync(ITelegramBotClient client, long chatId, TelegramUserRequestFormEntity formEntity, CancellationToken cancellationToken = default)
    {
        var currentForm = _formModels[formEntity.FormType];
        var currentStep = currentForm.Steps.ElementAt(formEntity.CurrentStep);
        IReplyMarkup markup = new ReplyKeyboardRemove();
        if (currentStep.Answers is not null)
            markup = new ReplyKeyboardMarkup(currentStep.Answers.Select(x => x.Select(y => new KeyboardButton(y))))
            {
                ResizeKeyboard = true
            };
        var questionMessage = await client.SendTextMessageAsync(chatId, await currentStep.Question(formEntity.Data), 
            replyMarkup: markup, cancellationToken: cancellationToken);
        if (currentStep.TimeBeforeQuestionDeletion.HasValue)
            _ = DeleteMessageAfterDelayAsync(client, chatId, questionMessage.MessageId,
                currentStep.TimeBeforeQuestionDeletion.Value, cancellationToken: cancellationToken);
    }
    
    private async Task<TelegramUserRequestFormEntity?> GetOneAsync(long telegramUserId, bool asNoTracking = false, 
        CancellationToken cancellationToken = default)
    {
        return await (asNoTracking ? _context.RequestForms.AsNoTracking() : _context.RequestForms)
            .FirstOrDefaultAsync(x => x.User.TelegramUserId.Equals(telegramUserId), cancellationToken);
    }
    
    private async Task DeleteMessageAfterDelayAsync(ITelegramBotClient client, long chatId, int messageId, TimeSpan delay, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken);
        await client.DeleteMessageAsync(chatId, messageId, cancellationToken: cancellationToken);
    }
}

public interface IFormRegistration
{
    FormModel ResolveForm();
}