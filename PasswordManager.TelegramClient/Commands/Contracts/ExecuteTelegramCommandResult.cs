namespace PasswordManager.TelegramClient.Commands.Contracts;

public class ExecuteTelegramCommandResult
{
    public Type? NextListener { get; set; }
    
    /// <summary>
    /// If exists, NextListener will be ignored
    /// </summary>
    public Type? NextRunner { get; set; }
}