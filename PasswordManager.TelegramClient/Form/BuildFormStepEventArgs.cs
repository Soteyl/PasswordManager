using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Form;

public class BuildFormStepEventArgs
{
    public TelegramUserDataEntity UserData { get; set; }
    
    public Dictionary<string, string> Data { get; set; }
    
    public FormStep Builder { get; set; }
    
    public TelegramFormMessageHandler FormMessageHandler { get; set; }
}