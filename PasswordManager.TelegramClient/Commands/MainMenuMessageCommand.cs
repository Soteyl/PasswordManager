using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.AddAccount;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands;

public class MainMenuMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache): MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(AddAccountConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId));
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.WrongMessageWarningBody, 
            replyMarkup: GetMarkup(MessageButtons.ShowMyAccounts), cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}