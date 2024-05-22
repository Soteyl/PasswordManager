using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.FormRegistrations.Handler;

namespace PasswordManager.TelegramClient.Form.Contracts;

public class BuildFormStepEventArgs
{
    public TelegramUserDataEntity UserData { get; set; }
    
    public Dictionary<string, string> Data { get; set; }
    
    public FormStep Builder { get; set; }
    
    public TelegramFormMessageHandler FormMessageHandler { get; set; }
}