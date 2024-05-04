using Google.Protobuf;
using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountStep6FinishMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache, 
    PasswordStorageService.PasswordStorageServiceClient passwordStorageService) : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await request.Client.DeleteMessageAsync(request.Message.Chat.Id, request.Message.MessageId, cancellationToken: cancellationToken);
        
        var account = memoryCache.Get<AddAccountRequest>(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId));
        if (account == null) return new ExecuteTelegramCommandResult();

        var masterPassword = request.Message.Text!;
        if (request.UserData.MasterPasswordHash != Cryptographer.GetHash(masterPassword))
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
                MessageBodies.WrongMasterPassword, replyMarkup: GetMarkup(MessageButtons.Cancel),
                cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult()
            {
                NextListener = GetType()
            };
        }
        
        var result = Cryptographer.Encrypt(account.Password, masterPassword);
        
        var response = await passwordStorageService.AddAccountAsync(new AddAccountCommand()
        {
            CredentialsHash = ByteString.CopyFrom(result.CipherText),
            CredentialsSalt = ByteString.CopyFrom(result.IV),
            Url = account.WebsiteUrl,
            User = account.Username,
            UserId = request.UserData.Id.ToString(),
            WebsiteNickname = account.WebsiteNickname
        }, cancellationToken: cancellationToken);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
            (response.Response.IsSuccess) ? MessageBodies.AddAccountSuccess : MessageBodies.InternalError, 
            replyMarkup: GetMarkup(MessageButtons.Return), cancellationToken: cancellationToken);
        
        memoryCache.Remove(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId));

        return new ExecuteTelegramCommandResult();
    }
}