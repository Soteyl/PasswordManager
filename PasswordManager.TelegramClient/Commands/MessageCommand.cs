using System.Globalization;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Commands.SetUpMasterPassword;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands;

public abstract class MessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formHandler): ITelegramCommand
{
    protected bool MasterPasswordNeeded { get; set; } = true;
    
    protected virtual List<string> Commands { get; } = new ();

    public virtual Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Commands.Contains(message.Text!));
    }

    public async Task<ExecuteTelegramCommandResult?> ExecuteAsync(Message message, ITelegramBotClient client, CancellationToken cancellationToken = default)
    {
        var userData = await userDataRepository.GetUserDataAsync(message.From.Id, cancellationToken);
        CultureInfo.CurrentCulture = userData.Locale.ToCulture();
        
        if (MasterPasswordNeeded && string.IsNullOrEmpty(userData.MasterPasswordHash))
        {
            await formHandler.StartFormRequestAsync<SetUpMasterPasswordFormRegistration>(client, message.From.Id, message.Chat.Id, cancellationToken);

            return new ExecuteTelegramCommandResult();
        }

        return await ExecuteCommandAsync(new ExecuteTelegramCommandRequest()
        {
            Message = message,
            Client = client,
            UserData = userData
        }, cancellationToken);
    }

    protected abstract Task<ExecuteTelegramCommandResult?> ExecuteCommandAsync(ExecuteTelegramCommandRequest request,
        CancellationToken cancellationToken = default);

    
    #region keyboard markup
    
    protected static ReplyKeyboardMarkup GetMarkup(IEnumerable<IEnumerable<string>> buttons)
    {
        return new(buttons.Select(row => row.Select(x => new KeyboardButton(x))))
        {
            ResizeKeyboard = true
        };
    }
    
    protected static ReplyKeyboardMarkup GetMarkup(params string[] buttons)
    {
        return GetMarkup(false, buttons);
    }
    
    protected static ReplyKeyboardMarkup GetMarkup(bool isVerticalAlignment = false, params string[] buttons)
    {
        if (isVerticalAlignment)
            return new(buttons.Select(x => new[] { new KeyboardButton(x) }))
            {
                ResizeKeyboard = true
            };
        
        return new(buttons.Select(x => new KeyboardButton(x)))
        {
            ResizeKeyboard = true
        };
    }
    
    #endregion
}