using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.GetAccountCredentials;

public class GetAccountCredentialsTypeMasterPasswordMessageCommand(IUserDataRepository userDataRepository, 
    PasswordStorageService.PasswordStorageServiceClient passwordStorageService, IMemoryCache memoryCache) : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        var splittedMessageData = request.Message.Text?.Split(" ");
        if (splittedMessageData is not { Length: 2 })
        {
            return new ExecuteTelegramCommandResult();
        }

        var account = await passwordStorageService.GetAccountByWebsiteNicknameAndUserAsync(
            new GetAccountByWebsiteNicknameAndUserRequest()
            {
                UserId = request.UserData.Id.ToString(),
                WebsiteNickname = splittedMessageData[0],
                AccountUser = splittedMessageData[1].Replace("(", "").Replace(")", "")
            }, cancellationToken: cancellationToken);
        
        if (!account.Response.IsSuccess || account.Account == null)
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id, MessageBodies.InternalError,
                replyMarkup: GetMarkup(MessageButtons.Cancel), cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult();
        }
        
        var creds = await passwordStorageService.GetAccountCredentialsAsync(new GetAccountCredentialsRequest()
        {
            AccountId = account.Account.AccountId,
            UserId = request.UserData.Id.ToString()
        }, cancellationToken: cancellationToken);
        
        if (!creds.Response.IsSuccess)
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id, MessageBodies.InternalError,
                replyMarkup: GetMarkup(MessageButtons.Cancel), cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult();
        }
        
        memoryCache.Set(CacheConstraints.GetGetAccountCredentialsContractCacheKey(request.Message.Chat.Id), creds);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, 
            MessageBodiesParametrized.GetCredentialsProvideMasterPassword(account.Account.Url, account.Account.WebsiteNickname, account.Account.User),
            replyMarkup: GetMarkup(MessageButtons.Cancel), cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult()
        {
            NextListener = typeof(FinalStepGetAccountCredentialsMessageCommand)
        };
    }
}