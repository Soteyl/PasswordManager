using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands;

public class CancelMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache, 
    ITelegramCommandResolver commandResolver, TelegramFormMessageHandler formMessageHandler) : MessageCommand(userDataRepository)
{
    protected override List<string> Commands { get; } = [MessageButtons.Cancel, MessageButtons.Return];

    protected override async Task<ExecuteTelegramCommandResult?> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId));
        memoryCache.Remove(CacheConstraints.GetCommandListenerCacheKey(request.UserData.TelegramUserId));
        await formMessageHandler.FinishCurrentFormAsync(request.UserData.TelegramUserId, cancellationToken);
        
        var mainMenu = await commandResolver.ResolveCommandAsync<MainMenuMessageCommand>(cancellationToken);
        return await mainMenu.ExecuteAsync(request.Message, request.Client, cancellationToken)!;
    }
}