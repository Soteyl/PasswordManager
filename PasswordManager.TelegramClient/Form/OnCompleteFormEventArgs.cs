using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Form;

public class OnCompleteFormEventArgs
{
    public long ChatId { get; set; }
    
    public TelegramUserDataEntity UserData { get; set; }
    
    public Dictionary<string, string> Answers { get; set; }
    
    public IMessengerClient Client { get; set; }
}