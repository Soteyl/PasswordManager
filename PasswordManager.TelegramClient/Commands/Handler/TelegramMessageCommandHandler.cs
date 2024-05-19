using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class TelegramMessageCommandHandler(IServiceProvider serviceProvider, TelegramFormMessageHandler formMessageHandler): IUpdateHandler, ITelegramCommandResolver
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
        if (update.Message.Text == MessageButtons.Cancel)
        {
            await Commands[typeof(CancelMessageCommand)].ExecuteAsync(update.Message, botClient, cancellationToken);
            return;
        }

        var hasActiveForm = await formMessageHandler.HasActiveFormAsync(update.Message.From!.Id, cancellationToken);
        if (hasActiveForm)
        {
            _ = formMessageHandler.HandleFormRequestAsync(botClient, update.Message, cancellationToken);
            return;
        }
        
        var command = Commands.Values.FirstOrDefault(x => x.IsMatchAsync(update.Message, cancellationToken).Result)
                      ?? _wrongMessageCommand;
        
        _ = command.ExecuteAsync(update.Message, botClient, cancellationToken);
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
}