using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class QuestionMessageCommand(IUserDataRepository userDataRepository) : MessageCommand(userDataRepository)
{
    protected override Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}