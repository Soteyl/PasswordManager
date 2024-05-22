namespace PasswordManager.TelegramClient.Form;

public class FormValidateResult
{
    public bool IsSuccess { get; set; }
    
    public string? Error { get; set; }
    
    public string? ValidResult { get; set; }
}