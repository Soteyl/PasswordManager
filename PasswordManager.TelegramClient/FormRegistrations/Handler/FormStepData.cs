namespace PasswordManager.TelegramClient.FormRegistrations.Handler;

public class FormStepData
{
    public string? Message { get; set; }
    
    public int? MessageId { get; set; }
    
    public long UserId { get; set; }
    
    public long ChatId { get; set; }
}