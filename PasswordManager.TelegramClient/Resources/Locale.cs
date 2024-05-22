using System.Globalization;

namespace PasswordManager.TelegramClient.Resources;

public enum Locale
{
    English,
    Ukrainian
}

public static class LocaleExtensions
{
    public static CultureInfo ToCulture(this Locale locale)
    {
        return locale switch
        {
            Locale.English => new CultureInfo("en-US"),
            Locale.Ukrainian => new CultureInfo("uk-UA"),
            _ => new CultureInfo("en-US")
        };
    }
}