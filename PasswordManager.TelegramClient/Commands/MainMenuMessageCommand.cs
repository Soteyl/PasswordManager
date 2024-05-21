using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands;

public class MainMenuMessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formMessageHandler)
    : MessageCommand(userDataRepository, formMessageHandler)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.WrongMessageWarningBody, 
            replyMarkup: GetMarkup(MessageButtons.ShowMyAccounts), cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}