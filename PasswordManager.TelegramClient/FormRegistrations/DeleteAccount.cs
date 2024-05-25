using Newtonsoft.Json;
using PasswordManager.TelegramClient.Common.Keyboard;
using PasswordManager.TelegramClient.Common.Validation;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class DeleteAccount(PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
{
    private const string Account = "account";
    private const string WebsiteNickname = "websiteNickname";
    
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
                var message = MessageBodies.ChooseAccountForCredentials;

                if (!accounts.Response.IsSuccess)
                    message = MessageBodies.InternalError;
                else if (accounts.Accounts.Count == 0)
                    message = MessageBodies.YouHaveNoAccounts;

                var accountsMarkup = accounts.Accounts.Select(x => $"{x.WebsiteNickname} ({x.User})").ToList();
                accountsMarkup.Add(MessageButtons.Cancel);

                return s.Builder
                        .WithQuestion(message)
                        .WithAnswers(accountsMarkup.Select(x => new[]
                        {
                            x
                        }))
                        .OnlyButtonAnswer()
                        .ValidateAnswer((args, ct) => Validators.Account(args, passwordStorageService, ct))
                        .WithAnswerKey(Account);
            }).AddStep(s =>
            {
                var account = JsonConvert.DeserializeObject<AccountInfo>(s.Data[Account])!;
                return s.Builder
                        .WithQuestion(MessageBodiesParametrized.DeleteAccountConfirmation(account.Url, 
                            account.WebsiteNickname, account.User))
                        .WithAnswerKey(WebsiteNickname)
                        .WithAnswerRow(MessageButtons.Cancel)
                        .ValidateAnswer(ValidateWebsiteNickname);
            })
            .OnComplete(OnComplete).Build();
    }

    private FormValidateResult ValidateWebsiteNickname(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var account = JsonConvert.DeserializeObject<AccountInfo>(eventArgs.Context![Account])!;
        var success = eventArgs.Answer.Equals(account?.WebsiteNickname);

        return new FormValidateResult()
        {
            IsSuccess = success,
            Error = MessageBodies.WrongDeleteAccountConfirmation
        };
    }

    private async Task OnComplete(OnCompleteFormEventArgs eventargs, CancellationToken cancellationtoken)
    {
        var account = JsonConvert.DeserializeObject<AccountInfo>(eventargs.Data[Account])!;
        var returnMarkup = new KeyboardBuilder().Return().Build();

        var deleteAccountResponse = await passwordStorageService.DeleteAccountAsync(new DeleteAccountCommand()
        {
            AccountId = account.AccountId,
            UserId = eventargs.UserData.InternalId.ToString()
        }, cancellationToken: cancellationtoken);
        
        await eventargs.Client.SendMessageAsync( deleteAccountResponse.IsSuccess ? MessageBodies.AccountDeleted : MessageBodies.InternalError,
            eventargs.ChatId, answers: returnMarkup, cancellationToken: cancellationtoken);
    }
}