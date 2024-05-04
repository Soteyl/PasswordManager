using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountStep4PasswordMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache) : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var account = memoryCache.Get<AddAccountRequest>(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId));
        if (account == null) return new ExecuteTelegramCommandResult();
        
        var user = request.Message.Text;
        account.Username = user;
        memoryCache.Set(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId), account,
            CacheConstraints.AddAccountContractExpiration);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
            MessageBodies.SendPasswordToAddAccount, replyMarkup: GetMarkup(MessageButtons.Cancel),
            cancellationToken: cancellationToken);
        return new ExecuteTelegramCommandResult()
        {   
            NextListener = typeof(AddAccountStep5MasterPasswordMessageCommand)
        };
    }
}