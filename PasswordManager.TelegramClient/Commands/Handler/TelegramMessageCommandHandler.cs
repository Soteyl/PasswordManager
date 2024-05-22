using PasswordManager.TelegramClient.Commands.SetUpMasterPassword;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.Handler;

public class TelegramMessageCommandHandler(TelegramFormMessageHandler formMessageHandler, IMessengerClient client, 
    IUserDataRepository userDataRepository): IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        var form = await formMessageHandler.GetFormByMessageAsync(update.Message, cancellationToken);
        
        if (form == typeof(CancelFormRegistration))
        {
            _ = formMessageHandler.StartFormRequestAsync<CancelFormRegistration>(update.Message.From!.Id, 
                update.Message.Chat.Id, cancellationToken);
            return;
        }

        var hasActiveForm = await formMessageHandler.GetActiveFormAsync(update.Message.From!.Id, cancellationToken);
        if (hasActiveForm != typeof(SetUpMasterPasswordFormRegistration) 
            && await userDataRepository.GetUserDataAsync(update.Message.From!.Id, cancellationToken) 
            is { MasterPasswordHash: null })
        {
            _ = formMessageHandler.StartFormRequestAsync<SetUpMasterPasswordFormRegistration>(update.Message.From!.Id, 
                update.Message.Chat.Id, cancellationToken);
        }
        
        if (hasActiveForm is not null)
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