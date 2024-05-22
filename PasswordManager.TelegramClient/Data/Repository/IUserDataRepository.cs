using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Data.Repository;

public interface IUserDataRepository
{
    Task<TelegramUserDataEntity> GetUserDataAsync(long telegramUserId, CancellationToken cancellationToken = default);
    
    Task ChangeLocaleAsync(long telegramUserId, Locale locale, CancellationToken cancellationToken = default);
    
    Task ChangeMasterPasswordAsync(long telegramUserId, string masterPassword, CancellationToken cancellationToken = default);
}