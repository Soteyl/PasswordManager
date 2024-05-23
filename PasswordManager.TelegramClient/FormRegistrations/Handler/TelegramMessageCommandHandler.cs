using System.Globalization;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Messenger;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.FormRegistrations.Handler;

public class TelegramMessageCommandHandler(TelegramFormMessageHandler formMessageHandler, 
    IUserDataRepository userDataRepository): IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        var form = await formMessageHandler.GetFormByMessageAsync(update.Message, cancellationToken);
        var user = await userDataRepository.GetUserDataAsync(update.Message.From!.Id, cancellationToken);

        CultureInfo.CurrentUICulture = user.Locale.ToCulture();

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
            _ = formMessageHandler.HandleFormRequestAsync(update.Message, cancellationToken);
            return;
        }

        await formMessageHandler.StartFormRequestAsync(form, update.Message.From.Id, update.Message.Chat.Id, cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}