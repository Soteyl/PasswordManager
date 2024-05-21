using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class TelegramMessageCommandHandler(TelegramFormMessageHandler formMessageHandler, 
    ITelegramCommandResolver commandResolver): IUpdateHandler
{
    private ITelegramCommand _wrongMessageCommand;
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        if (update.Message.Text == MessageButtons.Cancel || update.Message.Text == MessageButtons.Return)
        {
            (await commandResolver.ResolveCommandAsync<CancelMessageCommand>(cancellationToken))
                .ExecuteAsync(update.Message, botClient, cancellationToken);
            return;
        }

        var hasActiveForm = await formMessageHandler.HasActiveFormAsync(update.Message.From!.Id, cancellationToken);
        if (hasActiveForm)
        {
            _ = formMessageHandler.HandleFormRequestAsync(botClient, update.Message, cancellationToken);
            return;
        }
        
        var command = await commandResolver.ResolveCommandByMessageAsync(update.Message, cancellationToken);
        
        _ = command.ExecuteAsync(update.Message, botClient, cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}