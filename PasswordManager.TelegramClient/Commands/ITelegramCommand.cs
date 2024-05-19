using PasswordManager.TelegramClient.Commands.Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands;

public interface ITelegramCommand
{
    Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default);
    
    Task<ExecuteTelegramCommandResult?> ExecuteAsync(Message message, ITelegramBotClient client, CancellationToken cancellationToken = default);
}