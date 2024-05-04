using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class TelegramMessageCommandHandler(IServiceProvider serviceProvider, IMemoryCache memoryCache): IUpdateHandler, ITelegramCommandResolver
{
    private Dictionary<Type, ITelegramCommand>? _commands = null;

    private Dictionary<Type, ITelegramCommand> Commands
    {
        get
        {
            if (_commands is null) RegisterCommands();
            return _commands!;
        }
    }
    
    private ITelegramCommand _wrongMessageCommand;
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;

        var cachedListener = memoryCache.Get<Type>(GetListenerCacheKey(update.Message.Chat.Id));
        if (cachedListener is not null)
            memoryCache.Remove(GetListenerCacheKey(update.Message.Chat.Id));
        
        var command = (cachedListener is null 
                          ? Commands.Values.FirstOrDefault(x => x.IsMatchAsync(update.Message, cancellationToken).Result)
                          : Commands.GetValueOrDefault(cachedListener)) 
                      ?? _wrongMessageCommand;
        
        var result = await command.ExecuteAsync(update.Message, botClient, cancellationToken);

        if (result.NextListener is not null)
        {
            memoryCache.Set(GetListenerCacheKey(update.Message.Chat.Id), result.NextListener, TimeSpan.FromMinutes(10));
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ITelegramCommand> ResolveCommandAsync<T>(CancellationToken cancellationToken = default)
        where T: ITelegramCommand
    {
        return Task.FromResult(Commands[typeof(T)]);
    }
    
    private void RegisterCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(ITelegramCommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        _commands = commandTypes.Select(x => 
                (ITelegramCommand)ActivatorUtilities.CreateInstance(serviceProvider, x))
            .ToDictionary(x => x.GetType(), x => x);

        _wrongMessageCommand = ResolveCommandAsync<MainMenuMessageCommand>().Result;
    }
    
    private static string GetListenerCacheKey(long chatId) => $"Listener_{chatId}";
}