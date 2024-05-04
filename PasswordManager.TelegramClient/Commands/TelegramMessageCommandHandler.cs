using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands;

public class TelegramMessageCommandHandler: IUpdateHandler, ITelegramCommandResolver
{
    private readonly IMemoryCache _memoryCache;
    
    private readonly Dictionary<Type, ITelegramCommand> _commands = new();

    private readonly ITelegramCommand _wrongMessageCommand;

    public TelegramMessageCommandHandler(IServiceProvider serviceProvider, IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(ITelegramCommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        _commands = commandTypes.Select(x => 
                (ITelegramCommand)ActivatorUtilities.CreateInstance(serviceProvider, x))
            .ToDictionary(x => x.GetType(), x => x);

        _wrongMessageCommand = ResolveCommandAsync<MainMenuMessageCommand>().Result;
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;

        var cachedListener = _memoryCache.Get<Type>(GetListenerCacheKey(update.Message.Chat.Id));
        
        var command = (cachedListener is null 
                          ? _commands.Values.FirstOrDefault(x => x.IsMatchAsync(update.Message, cancellationToken).Result)
                          : _commands.GetValueOrDefault(cachedListener)) 
                      ?? _wrongMessageCommand;
        
        var result = await command.ExecuteAsync(update.Message, botClient, cancellationToken);

        if (result.NextListener is not null)
        {
            _memoryCache.Set(GetListenerCacheKey(update.Message.Chat.Id), result.NextListener, TimeSpan.FromMinutes(10));
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ITelegramCommand> ResolveCommandAsync<T>(CancellationToken cancellationToken = default)
        where T: ITelegramCommand
    {
        return Task.FromResult(_commands[typeof(T)]);
    }
    
    private static string GetListenerCacheKey(long chatId) => $"Listener_{chatId}";
}