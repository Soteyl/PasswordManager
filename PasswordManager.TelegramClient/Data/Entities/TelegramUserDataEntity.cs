using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Data.Entities;

public class TelegramUserDataEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public long TelegramUserId { get; set; }
    
    public string MasterPasswordHash { get; set; } 
    
    public Locale Locale { get; set; }
}