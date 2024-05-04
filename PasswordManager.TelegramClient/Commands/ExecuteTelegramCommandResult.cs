namespace PasswordManager.TelegramClient.Commands;

public class ExecuteTelegramCommandResult
{
    public ITelegramCommand? NextListener { get; set; }
}