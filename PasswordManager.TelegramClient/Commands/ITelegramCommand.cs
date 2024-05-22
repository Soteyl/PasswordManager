using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands;

public interface ITelegramCommand
{
    Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default);
    
    Task<ExecuteTelegramCommandResult?> ExecuteAsync(Message message, IMessengerClient client, CancellationToken cancellationToken = default);
}