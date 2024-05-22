using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Form.Contracts;

public class ValidateAnswerEventArgs
{
    public TelegramUserDataEntity UserData { get; set; }
    
    public string Answer { get; set; }
    
    public Dictionary<string, string>? Context { get; set; }
}