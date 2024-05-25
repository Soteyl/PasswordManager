using System.Globalization;
using Microsoft.Extensions.Logging;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Messenger;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.FormRegistrations.Handler;

public class TelegramMessageCommandHandler(TelegramFormMessageHandler formMessageHandler, 
    IUserDataRepository userDataRepository, ILogger<TelegramMessageCommandHandler> logger): IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        var user = await userDataRepository.GetUserDataAsync(update.Message.From!.Id, cancellationToken);
        CultureInfo.CurrentUICulture = user.Locale.ToCulture();
        var form = await formMessageHandler.GetFormByMessageAsync(update.Message, cancellationToken);

        var activeForm = await formMessageHandler.GetActiveFormAsync(update.Message.From!.Id, cancellationToken);
        if (activeForm != typeof(SetUpMasterPassword) 
            && user is { MasterPasswordHash: null })
        {
            _ = formMessageHandler.StartFormRequestAsync<SetUpMasterPassword>(update.Message.From!.Id, 
                update.Message.Chat.Id, cancellationToken);
            return;
        }
        
        if (form == typeof(Cancel))
        {
            _ = formMessageHandler.StartFormRequestAsync<Cancel>(update.Message.From!.Id, 
                update.Message.Chat.Id, cancellationToken);
            return;
        }
        
        if (activeForm is not null)
        {
            _ = formMessageHandler.HandleFormRequestAsync(new FormStepData()
            {
                Message = update.Message.Text,
                MessageId = update.Message.MessageId,
                UserId = update.Message.From.Id,
                ChatId = update.Message.Chat.Id
            }, cancellationToken);
            return;
        }

        await formMessageHandler.StartFormRequestAsync(form, update.Message.From.Id, update.Message.Chat.Id, cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception.ToString());  
        return Task.CompletedTask;
    }
}