using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands;

public class CancelMessageCommand: MessageCommand
{
    private readonly TelegramFormMessageHandler _formMessageHandler;
    private readonly IMemoryCache _memoryCache;
    private readonly ITelegramCommandResolver _commandResolver;

    public CancelMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache, 
        ITelegramCommandResolver commandResolver, TelegramFormMessageHandler formMessageHandler): base(userDataRepository, formMessageHandler)
    {
        _memoryCache = memoryCache;
        _commandResolver = commandResolver;
        _formMessageHandler = formMessageHandler;
        MasterPasswordNeeded = false;
    }

    protected override List<string> Commands { get; } = [MessageButtons.Cancel, MessageButtons.Return];

    protected override async Task<ExecuteTelegramCommandResult?> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await _formMessageHandler.FinishCurrentFormAsync(request.UserData.TelegramUserId, cancellationToken);
        _memoryCache.Remove(CacheConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId));
        _memoryCache.Remove(CacheConstraints.GetCommandListenerCacheKey(request.UserData.TelegramUserId));
        await _formMessageHandler.FinishCurrentFormAsync(request.UserData.TelegramUserId, cancellationToken);
        
        var mainMenu = await _commandResolver.ResolveCommandAsync<MainMenuMessageCommand>(cancellationToken);
        return await mainMenu.ExecuteAsync(request.Message, request.Client, cancellationToken)!;
    }
}