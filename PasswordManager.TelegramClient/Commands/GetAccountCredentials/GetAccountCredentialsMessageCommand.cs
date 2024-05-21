using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.GetAccountCredentials;

public class GetAccountCredentialsMessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formHandler) 
    : MessageCommand(userDataRepository, formHandler)
{
    protected override List<string> Commands { get; } = [MessageButtons.GetAccountCredentials];

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await formHandler.StartFormRequestAsync<GetAccountCredentialsFormRegistration>(request.Client, request.UserData.TelegramUserId, request.Message.Chat.Id, cancellationToken);
        
        return new ExecuteTelegramCommandResult();
    }
}