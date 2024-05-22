using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.TelegramClient.Commands.Handler;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands;

public class TelegramMessageCommandResolver(IServiceProvider serviceProvider): ITelegramCommandResolver
{
    private Dictionary<Type, ITelegramCommand>? _commands = null;
    
    private ITelegramCommand _wrongMessageCommand;

    private Dictionary<Type, ITelegramCommand> Commands
    {
        get
        {
            if (_commands is null) RegisterCommands();
            return _commands!;
        }
    }

    public Task<ITelegramCommand> ResolveCommandAsync<T>(CancellationToken cancellationToken = default)
        where T: ITelegramCommand
    {
        return Task.FromResult(Commands[typeof(T)]);
    }

    public async Task<ITelegramCommand> ResolveCommandByMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        return Commands.Values.FirstOrDefault(x => x.IsMatchAsync(message, cancellationToken).Result)
            ?? _wrongMessageCommand;
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
}