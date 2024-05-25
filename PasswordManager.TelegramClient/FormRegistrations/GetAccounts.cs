using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class GetAccounts(PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
{
    private const string Account = "account";
    
    public FormModel ResolveForm()
    {
        return new FormBuilder()
            .AddStep(s =>
            {
                var accounts = passwordStorageService.GetAccountsAsync(new GetAccountsRequest()
                {
                    UserId = s.UserData.InternalId.ToString(),
                    Limit = 100
                }).ResponseAsync.Result;

                var message = accounts.Response.IsSuccess 
                    ? accounts.Accounts.Count == 0 
                        ? MessageBodies.YouHaveNoAccounts
                        : MessageBodiesParametrized.AccountsList(accounts.Accounts.ToList())
                    : MessageBodies.InternalError;

                var messageButtons = new List<List<string>>()
                {
                    new() { MessageButtons.AddAccount },
                    new() { MessageButtons.Return }
                };

                if (accounts.Accounts.Count > 0)
                {
                    messageButtons[0].Add(MessageButtons.GetAccountCredentials);
                    messageButtons[0].Add(MessageButtons.DeleteAccount);
                    messageButtons[1].Insert(0, MessageButtons.ChangeAccount);
                }

                return s.Builder
                        .WithQuestion(message)
                        .DisableWebPagePreview()
                        .WithAnswers(messageButtons)
                        .OnlyButtonAnswer()
                        .WithAnswerKey(Account)
                        .ExecuteAnotherForm<AddAccount>(MessageButtons.AddAccount)
                        .ExecuteAnotherForm<GetAccountCredentials>(MessageButtons.GetAccountCredentials)
                        .ExecuteAnotherForm<DeleteAccount>(MessageButtons.DeleteAccount)
                        .ExecuteAnotherForm<ChangeAccount>(MessageButtons.ChangeAccount);
            })
            .Build();
    }
}