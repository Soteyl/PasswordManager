using System.Globalization;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands;

public abstract class MessageCommand(IUserDataRepository userDataRepository, ITelegramCommandResolver commandResolver): ITelegramCommand
{
    public abstract Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default);

    public async Task<ExecuteTelegramCommandResult> ExecuteAsync(Message message, ITelegramBotClient client, CancellationToken cancellationToken = default)
    {
        var userData = await userDataRepository.GetUserDataAsync(message.Chat.Id, cancellationToken);
        CultureInfo.CurrentCulture = userData.Locale.ToCulture();
        
        if (string.IsNullOrEmpty(userData.MasterPasswordHash))
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { MessageBodies.SetUpMasterPasswordMessageBody },
            })
            {
                ResizeKeyboard = true
            };
            
            await client.SendTextMessageAsync(message.Chat.Id, MessageBodies.WrongMessageWarningBody, 
                replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);

            return new ExecuteTelegramCommandResult()
            {
                NextListener = await commandResolver.ResolveCommandAsync<SetMasterPasswordMessageCommand>(cancellationToken)
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
}