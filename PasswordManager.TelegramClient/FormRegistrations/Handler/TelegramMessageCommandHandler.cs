using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Messenger;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.FormRegistrations.Handler;

public class TelegramMessageCommandHandler(TelegramFormMessageHandler formMessageHandler, IMessengerClient client, 
    IUserDataRepository userDataRepository): IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        var form = await formMessageHandler.GetFormByMessageAsync(update.Message, cancellationToken);

        var activeForm = await formMessageHandler.GetActiveFormAsync(update.Message.From!.Id, cancellationToken);
        if (activeForm != typeof(SetUpMasterPassword) 
            && await userDataRepository.GetUserDataAsync(update.Message.From!.Id, cancellationToken)
                is { MasterPasswordHash: null })
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
            _ = formMessageHandler.HandleFormRequestAsync(client, update.Message, cancellationToken);
            return;
        }

        await formMessageHandler.StartFormRequestAsync(form, update.Message.From.Id, update.Message.Chat.Id, cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}