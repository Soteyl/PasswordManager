using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountWebsiteNicknameMessageCommand(IUserDataRepository userDataRepository) : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
            MessageBodies.SendWebsiteNicknameToAddAccount, replyMarkup: GetMarkup(MessageButtons.Cancel),
            cancellationToken: cancellationToken);
        return new ExecuteTelegramCommandResult()
        {
            NextListener = typeof(AddAccountWebsiteNicknameMessageCommand)
        };
    }
}