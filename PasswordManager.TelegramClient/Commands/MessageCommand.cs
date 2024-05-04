using System.Globalization;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands;

public abstract class MessageCommand(IUserDataRepository userDataRepository): ITelegramCommand
{
    protected bool MasterPasswordNeeded { get; set; } = true;
    
    protected virtual List<string> Commands { get; } = new ();

    public virtual Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Commands.Contains(message.Text!));
    }

    public async Task<ExecuteTelegramCommandResult> ExecuteAsync(Message message, ITelegramBotClient client, CancellationToken cancellationToken = default)
    {
        var userData = await userDataRepository.GetUserDataAsync(message.Chat.Id, cancellationToken);
        CultureInfo.CurrentCulture = userData.Locale.ToCulture();
        
        if (MasterPasswordNeeded && string.IsNullOrEmpty(userData.MasterPasswordHash))
        {
            await client.SendTextMessageAsync(message.Chat.Id, MessageBodies.SetUpMasterPasswordMessageBody, 
                replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);

            return new ExecuteTelegramCommandResult()
            {
                NextListener = typeof(SetMasterPasswordMessageCommand)
            };
        }

        if (message.Text == MessageButtons.Cancel)
        {
            return new ExecuteTelegramCommandResult()
            {
                NextListener = typeof(MainMenuMessageCommand)
            };
        }

        return await ExecuteCommandAsync(new ExecuteTelegramCommandRequest()
        {
            Message = message,
            Client = client,
            UserData = userData
        }, cancellationToken);
    }

    protected abstract Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request,
        CancellationToken cancellationToken = default);

    protected static ReplyKeyboardMarkup GetMarkup(params string[] buttons)
    {
        return new(buttons.Select(x => new KeyboardButton(x)))
        {
            ResizeKeyboard = true
        };
    }
}