namespace PasswordManager.TelegramClient.Commands.Handler;

public static class CacheConstraints
{
    public static string GetAddAccountContractCacheKey(long userId) => $"AddAccount_{userId}";
    
    public static string GetCommandListenerCacheKey(long chatId) => $"Listener_{chatId}";
    
    public static TimeSpan AddAccountContractExpiration => TimeSpan.FromMinutes(5);
}