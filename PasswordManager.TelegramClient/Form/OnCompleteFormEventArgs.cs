using PasswordManager.TelegramClient.Data.Entities;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Form;

public class OnCompleteFormEventArgs
{
    public long ChatId { get; set; }
    
    public TelegramUserDataEntity UserData { get; set; }
    
    public Dictionary<string, string> Answers { get; set; }
    
    public ITelegramBotClient Client { get; set; }
}