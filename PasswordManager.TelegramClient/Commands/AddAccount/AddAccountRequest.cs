namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountRequest
{
    public string WebsiteUrl { get; set; }
    
    public string WebsiteNickname { get; set; }
    
    public string Username { get; set; }
    
    public string Password { get; set; }
}