using Microsoft.Extensions.Caching.Memory;
using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountStep2WebsiteNicknameMessageCommand(IUserDataRepository userDataRepository, IMemoryCache memoryCache) : MessageCommand(userDataRepository)
{
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var url = request.Message.Text;
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
                MessageBodies.WrongUrlFormat, replyMarkup: GetMarkup(MessageButtons.Cancel),
                cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult()
            {
                NextListener = GetType()
            };
        }

        memoryCache.Set(AddAccountConstraints.GetAddAccountContractCacheKey(request.UserData.TelegramUserId),
            new AddAccountRequest()
            {
                WebsiteUrl = url
            }, AddAccountConstraints.AddAccountContractExpiration);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id,
            MessageBodies.SendWebsiteNicknameToAddAccount, replyMarkup: GetMarkup(MessageButtons.Cancel),
            cancellationToken: cancellationToken);
        return new ExecuteTelegramCommandResult()
        {   
            NextListener = typeof(AddAccountStep3UserMessageCommand)
        };
    }
}