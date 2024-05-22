using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Contracts;

public class ExecuteTelegramCommandRequest
{
    public Message Message { get; set; }

    public IMessengerClient Client { get; set; }
    
    public TelegramUserDataEntity UserData { get; set; }
}