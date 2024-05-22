using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Keyboard;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands;

public class MainMenuMessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formMessageHandler)
    : MessageCommand(userDataRepository, formMessageHandler)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await request.Client.SendMessageAsync(MessageBodies.WrongMessageWarningBody, request.Message.Chat.Id, 
            new KeyboardBuilder().AddRow(MessageButtons.ShowMyAccounts).Build(), cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}