using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Messenger;

namespace PasswordManager.TelegramClient.Form.Contracts;

public class OnCompleteFormEventArgs
{
    public long ChatId { get; set; }
    
    public TelegramUserDataEntity UserData { get; set; }
    
    public Dictionary<string, string> Data { get; set; }
    
    public IMessengerClient Client { get; set; }
    
    public TelegramFormMessageHandler FormMessageHandler { get; set; }
}