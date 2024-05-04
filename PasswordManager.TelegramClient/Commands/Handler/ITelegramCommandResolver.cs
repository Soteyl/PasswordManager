namespace PasswordManager.TelegramClient.Commands.Handler;

public interface ITelegramCommandResolver
{
    Task<ITelegramCommand> ResolveCommandAsync<T>(CancellationToken cancellationToken = default)
        where T: ITelegramCommand;
}