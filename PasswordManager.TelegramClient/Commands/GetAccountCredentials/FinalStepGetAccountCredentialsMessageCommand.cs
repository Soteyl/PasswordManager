using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands.GetAccountCredentials;

public class FinalStepGetAccountCredentialsMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache) 
    : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        await request.Client.DeleteMessageAsync(request.Message.Chat.Id, request.Message.MessageId,
            cancellationToken: cancellationToken);
        if (request.UserData.MasterPasswordHash != Cryptographer.GetHash(request.Message.Text!))
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
                MessageBodies.WrongMasterPassword, replyMarkup: GetMarkup(MessageButtons.Cancel),
                cancellationToken: cancellationToken);
            
            return new ExecuteTelegramCommandResult()
            {
                NextListener = GetType()
            };
        }
        
        var data = memoryCache.Get<GetAccountCredentialsResult>(
            CacheConstraints.GetGetAccountCredentialsContractCacheKey(request.Message.Chat.Id));
        
        if (data is null)
        {
            return new ExecuteTelegramCommandResult();
        }
        
        var decryptedPassword = Cryptographer.Decrypt(data.CredentialsHash.ToByteArray(), 
            data.CredentialsSalt.ToByteArray(), 
            request.Message.Text!);

        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, MessageBodies.HereIsYourPassword, 
            cancellationToken: cancellationToken);

        var passwordMessage = await request.Client.SendTextMessageAsync(request.Message.Chat.Id, decryptedPassword,
            replyMarkup: GetMarkup(MessageButtons.Return), cancellationToken: cancellationToken);
        
        await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        
        await request.Client.DeleteMessageAsync(request.Message.Chat.Id, passwordMessage.MessageId,
            cancellationToken: cancellationToken);
        
        return new ExecuteTelegramCommandResult();
    }
}