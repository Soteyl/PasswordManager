using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands;

public class MainMenuMessageCommand(IUserDataRepository userDataRepository): MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.WrongMessageWarningBody, 
            replyMarkup: GetMarkup(MessageButtons.ShowMyAccounts), cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}