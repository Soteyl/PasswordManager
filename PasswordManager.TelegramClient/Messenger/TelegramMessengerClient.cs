using System.Text.RegularExpressions;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Messenger;

public partial class TelegramMessengerClient(ITelegramBotClient client): IMessengerClient
{
    public async Task<Message> SendMessageAsync(string message, long chatId, 
        IEnumerable<IEnumerable<string>>? answers = null,
        bool disableWebPagePreview = false,
        CancellationToken cancellationToken = default)
    {
        return await client.SendTextMessageAsync(chatId, message, 
            replyMarkup: GetMarkup(answers), 
            parseMode: ParseMode.Markdown,
            disableWebPagePreview: disableWebPagePreview,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteMessageAsync(int messageId, long chatId, CancellationToken cancellationToken = default)
    {
        await client.DeleteMessageAsync(chatId, messageId, cancellationToken);
    }
    
    private IReplyMarkup GetMarkup(IEnumerable<IEnumerable<string>>? answers)
    {
        answers = answers?.ToList();
        return answers is not null && answers.Any() 
            ? new ReplyKeyboardMarkup(answers.Select(x => x.Select(y => new KeyboardButton(y))))
            {
                ResizeKeyboard = true
            }
            : new ReplyKeyboardRemove();
    }
}