using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Handler;

public interface ITelegramCommandResolver
{
    Task<ITelegramCommand> ResolveCommandAsync<T>(CancellationToken cancellationToken = default)
        where T: ITelegramCommand;

    Task<ITelegramCommand> ResolveCommandByMessageAsync(Message message, CancellationToken cancellationToken = default);
}