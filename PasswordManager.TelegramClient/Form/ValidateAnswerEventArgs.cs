using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Form;

public class ValidateAnswerEventArgs
{
    public TelegramUserDataEntity UserData { get; set; }
    
    public string Answer { get; set; }
}