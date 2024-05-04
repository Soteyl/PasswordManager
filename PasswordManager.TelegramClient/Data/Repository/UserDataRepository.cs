using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Data.Repository;

public class UserDataRepository(TelegramClientContext context, IMemoryCache cache): IUserDataRepository
{
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    
    public async Task<TelegramUserDataEntity> GetUserDataAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(GetCacheKey(telegramUserId), out TelegramUserDataEntity? userData))
        {
            return userData!;
        }

        userData = await context.TelegramUserData.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);
        
        if (userData == null)
        {
            userData = new TelegramUserDataEntity()
            {
                TelegramUserId = telegramUserId
            };
            await context.AddAsync(userData, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        
        cache.Set(GetCacheKey(telegramUserId), userData, _cacheExpiration);

        return userData!;
    }
    
    public async Task ChangeLocaleAsync(long telegramUserId, Locale locale, CancellationToken cancellationToken = default)
    {
        var userData = await context.TelegramUserData.AsNoTracking()
            .FirstAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);
        
        userData.Locale = locale;
        
        await context.SaveChangesAsync(cancellationToken);
        
        cache.Set(GetCacheKey(telegramUserId), userData, _cacheExpiration);
    }

    public async Task ChangeMasterPasswordAsync(long telegramUserId, string masterPassword,
        CancellationToken cancellationToken = default)
    {
        var userData = await context.TelegramUserData
            .FirstAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);
        
        userData.MasterPasswordHash = Cryptographer.GetHash(masterPassword);
        
        await context.SaveChangesAsync(cancellationToken);
        
        cache.Set(GetCacheKey(telegramUserId), userData, _cacheExpiration);
    }

    private static string GetCacheKey(long telegramUserId) => $"UserData_{telegramUserId}";
}