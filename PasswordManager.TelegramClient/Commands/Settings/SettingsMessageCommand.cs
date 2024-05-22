using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.Settings;

public class SettingsMessageCommand(IUserDataRepository userDataRepository, TelegramFormMessageHandler formHandler): MessageCommand(userDataRepository, formHandler)
{
    protected override List<string> Commands { get; } = [MessageButtons.Settings];

    protected override async Task<ExecuteTelegramCommandResult?> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await formHandler.StartFormRequestAsync<SettingsFormRegistration>(request.Message.From!.Id, request.Message.Chat.Id, cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}