using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace PasswordManager.TelegramClient.Background;

public class TelegramUpdatesReceiver(ITelegramBotClient client, IUpdateHandler updateHandler, 
    ILogger<TelegramUpdatesReceiver> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await client.ReceiveAsync(updateHandler, cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }
    }
}