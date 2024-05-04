using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.GetAccountCredentials;

public class GetAccountCredentialsChooseAccountMessageCommand(IUserDataRepository userDataRepository, PasswordStorageService.PasswordStorageServiceClient passwordStorageService) : MessageCommand(userDataRepository)
{
    protected override List<string> Commands { get; } = [MessageButtons.GetAccountCredentials];

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await passwordStorageService.GetAccountsAsync(new GetAccountsRequest()
        {
            UserId = request.UserData.Id.ToString(),
            Limit = 100
        }, cancellationToken: cancellationToken);
        var message = MessageBodies.ChooseAccountForCredentials;
        if (!accounts.Response.IsSuccess)
            message = MessageBodies.InternalError;
        else if (accounts.Accounts.Count == 0)
            message = MessageBodies.YouHaveNoAccounts;
        
        var accountsMarkup = accounts.Accounts.Select(x => $"{x.WebsiteNickname} ({x.User})").ToList();
        accountsMarkup.Add(MessageButtons.Cancel);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, message,
            replyMarkup: GetMarkup(true, accountsMarkup.ToArray()), cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult()
        {
            NextListener = typeof(GetAccountCredentialsTypeMasterPasswordMessageCommand)
        };
    }
}