namespace PasswordManager.TelegramClient.Commands;

public interface ITelegramCommandResolver
{
    Task<ITelegramCommand> ResolveCommandAsync<T>(CancellationToken cancellationToken = default)
        where T: ITelegramCommand;
}