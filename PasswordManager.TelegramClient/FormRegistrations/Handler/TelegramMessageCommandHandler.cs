using System.Globalization;
using Microsoft.Extensions.Logging;
using PasswordManager.TelegramClient.Common.Keyboard;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Messenger;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.FormRegistrations.Handler;

public class TelegramMessageCommandHandler(TelegramFormMessageHandler formMessageHandler, 
    IUserDataRepository userDataRepository, 
    ILogger<TelegramMessageCommandHandler> logger, 
    IMessengerClient messengerClient): IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is null) return;
            var user = await userDataRepository.GetUserDataAsync(update.Message.From!.Id, cancellationToken);
            CultureInfo.CurrentUICulture = user.Locale.ToCulture();
            var form = await formMessageHandler.GetFormByMessageAsync(update.Message, cancellationToken);
            
            var activeForm = await formMessageHandler.GetActiveFormAsync(update.Message.From!.Id, cancellationToken);

            if (activeForm != typeof(SetUpMasterPassword)
                && user is { MasterPasswordHash: null })
            {
                _ = TryRunTask(formMessageHandler.StartFormRequestAsync<SetUpMasterPassword>(update.Message.From!.Id,
                    update.Message.Chat.Id, cancellationToken), update.Message.From.Id, cancellationToken);

                return;
            }

            if (activeForm is not null && form != typeof(MainMenu))
            {
                _ = TryRunTask(formMessageHandler.HandleFormRequestAsync(new FormStepData()
                {
                    Message = update.Message.Text,
                    MessageId = update.Message.MessageId,
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id
                }, cancellationToken), update.Message.From.Id, cancellationToken);

                return;
            }

            _ = TryRunTask(formMessageHandler.StartFormRequestAsync(form ?? typeof(MainMenu), update.Message.From.Id, update.Message.Chat.Id,
                cancellationToken), update.Message.From.Id, cancellationToken);
        }
        catch (Exception e) // dont throw exception again to avoid infinite loop
        {
            logger.LogError(e.ToString());

            if (update.Message?.From != null)
                await messengerClient.SendMessageAsync(MessageBodies.InternalError, update.Message.From.Id,
                    answers: new KeyboardBuilder().Return().Build(),
                    cancellationToken: cancellationToken);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception.ToString());  
        return Task.CompletedTask;
    }

    private async Task TryRunTask(Task task, long? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());

            if (userId.HasValue)
                await messengerClient.SendMessageAsync(MessageBodies.InternalError, userId.Value,
                    answers: new KeyboardBuilder().Return().Build(),
                    cancellationToken: cancellationToken);
        }
    }
}