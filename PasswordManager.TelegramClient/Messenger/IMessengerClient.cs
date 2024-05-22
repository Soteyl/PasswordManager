using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Telegram;

public interface IMessengerClient
{
    Task<Message> SendMessageAsync(
        string message, 
        long chatId, 
        IEnumerable<IEnumerable<string>>? answers = null,
        bool disableWebPagePreview = false,
        CancellationToken cancellationToken = default);

    Task DeleteMessageAsync(int messageId, long chatId, CancellationToken cancellationToken = default);
}