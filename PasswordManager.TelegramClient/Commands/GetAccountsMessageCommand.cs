using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands;

public class GetAccountsMessageCommand(IUserDataRepository userDataRepository,
    PasswordStorageService.PasswordStorageServiceClient storageService, TelegramFormMessageHandler formMessageHandler) 
    : MessageCommand(userDataRepository, formMessageHandler)
{
    protected override List<string> Commands { get; } = [MessageButtons.ShowMyAccounts];

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await storageService.GetAccountsAsync(new GetAccountsRequest()
        {
            Limit = 100,
            UserId = request.UserData.InternalId.ToString()
        }, cancellationToken: cancellationToken);

        var message = accounts.Response.IsSuccess 
            ? accounts.Accounts.Count == 0 
            ? MessageBodies.YouHaveNoAccounts
            : MessageBodiesParametrized.AccountsList(accounts.Accounts.ToList())
            : MessageBodies.InternalError;

        var messageButtons = new List<List<string>>()
        {
            new() { MessageButtons.AddAccount },
            new() { MessageButtons.Cancel }
        };

        if (accounts.Accounts.Count > 0)
        {
            messageButtons[0].Add(MessageButtons.GetAccountCredentials);
            messageButtons[0].Add(MessageButtons.DeleteAccount);
        }
        
        await request.Client.SendMessageAsync(message, request.Message.Chat.Id,
            messageButtons, disableWebPagePreview: true, cancellationToken: cancellationToken);
        return new ExecuteTelegramCommandResult();
    }
}