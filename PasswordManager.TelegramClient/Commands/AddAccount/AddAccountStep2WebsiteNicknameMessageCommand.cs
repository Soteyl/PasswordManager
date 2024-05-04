using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountStep2WebsiteNicknameMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache) : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var url = request.Message.Text;
        if (!IsValidUrl(url, out var validUrl))
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
                MessageBodies.WrongUrlFormat, replyMarkup: GetMarkup(MessageButtons.Cancel),
                cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult()
            {
                NextListener = GetType()
            };
        }

        memoryCache.Set(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId),
            new AddAccountRequest()
            {
                WebsiteUrl = validUrl
            }, CacheConstraints.AddAccountContractExpiration);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
            MessageBodies.SendWebsiteNicknameToAddAccount, replyMarkup: GetMarkup(MessageButtons.Cancel),
            cancellationToken: cancellationToken);
        return new ExecuteTelegramCommandResult()
        {   
            NextListener = typeof(AddAccountStep3UserMessageCommand)
        };
    }
    
    public static bool IsValidUrl(string url, out string validUrl)
    {
        if (!url.Contains("http")) url = "https://" + url;
        Uri.TryCreate(url, UriKind.Absolute, out Uri? validatedUri);
        validUrl = validatedUri?.ToString() ?? string.Empty;
        return validatedUri != null && (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
    }
}