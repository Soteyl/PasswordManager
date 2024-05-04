namespace PasswordManager.TelegramClient.Commands;

public class ExecuteTelegramCommandResult
{
    public Type? NextListener { get; set; }
    
    public Type? NextRunner { get; set; }
}