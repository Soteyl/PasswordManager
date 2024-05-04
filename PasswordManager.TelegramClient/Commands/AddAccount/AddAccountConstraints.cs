namespace PasswordManager.TelegramClient.Commands.AddAccount;

public static class AddAccountConstraints
{
    public static string GetAddAccountContractCacheKey(long userId) => $"AddAccount_{userId}";
    
    public static TimeSpan AddAccountContractExpiration => TimeSpan.FromMinutes(5);
}