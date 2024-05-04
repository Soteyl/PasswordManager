namespace PasswordManager.TelegramClient.Commands.Contracts;

public class ExecuteTelegramCommandResult
{
    public Type? NextListener { get; set; }
    
    public Type? NextRunner { get; set; }
}