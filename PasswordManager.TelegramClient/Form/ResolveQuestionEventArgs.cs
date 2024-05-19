using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Form;

public class ResolveQuestionEventArgs
{
    public TelegramUserDataEntity UserData { get; set; }
}