using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountMessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formHandler) : MessageCommand(userDataRepository)
{
    protected override List<string> Commands { get; } = [MessageButtons.AddAccount];

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await formHandler.StartFormRequestAsync<AddAccountFormRegistration>(request.Client, request.UserData.TelegramUserId, request.Message.Chat.Id, cancellationToken);
        
        return new ExecuteTelegramCommandResult();
    }
}