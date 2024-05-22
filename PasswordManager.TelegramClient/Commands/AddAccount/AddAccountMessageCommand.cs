using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountMessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formHandler) 
    : MessageCommand(userDataRepository, formHandler)
{
    private readonly TelegramFormMessageHandler _formHandler = formHandler;

    protected override List<string> Commands { get; } = [MessageButtons.AddAccount];

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await _formHandler.StartFormRequestAsync<AddAccountFormRegistration>(request.UserData.TelegramUserId, request.Message.Chat.Id, cancellationToken);
        
        return new ExecuteTelegramCommandResult();
    }
}