using PasswordManager.TelegramClient.Data.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Contracts;

public class ExecuteTelegramCommandRequest
{
    public Message Message { get; set; }

    public ITelegramBotClient Client { get; set; }
    
    public TelegramUserDataEntity UserData { get; set; }
}