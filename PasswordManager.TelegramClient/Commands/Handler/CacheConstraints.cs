namespace PasswordManager.TelegramClient.Commands.Handler;

public static class CacheConstraints
{
    public static string GetAddAccountContractCacheKey(long userId) => $"AddAccount_{userId}";
    
    public static string GetCommandListenerCacheKey(long chatId) => $"Listener_{chatId}";
    
    public static string GetGetAccountCredentialsContractCacheKey(long userId) => $"GetAccountCredentials_{userId}";
    
    public static string GetActiveInputFormCacheKey(long userId) => $"ActiveInputForm_{userId}";
    
    public static string GetActiveInputFormValidatorCacheKey(long userId) => $"ActiveInputFormValidator_{userId}";
    
    public static TimeSpan AddAccountContractExpiration => TimeSpan.FromMinutes(5);
}