using Newtonsoft.Json;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Keyboard;
using PasswordManager.TelegramClient.Resources;
using PasswordManager.TelegramClient.Validation;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.DeleteAccount;

public class DeleteAccountFormRegistration(PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
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
        var account = JsonConvert.DeserializeObject<AccountInfo>(eventargs.Answers[Account])!;
        var returnMarkup = KeyboardBuilder.GetMarkup(new KeyboardBuilder().Return().Build());

        var deleteAccountResponse = await passwordStorageService.DeleteAccountAsync(new DeleteAccountCommand()
        {
            AccountId = account.AccountId,
            UserId = eventargs.UserData.InternalId.ToString()
        }, cancellationToken: cancellationtoken);
        
        await eventargs.Client.SendTextMessageAsync(eventargs.ChatId, deleteAccountResponse.IsSuccess ? MessageBodies.AccountDeleted : MessageBodies.InternalError, 
            replyMarkup: returnMarkup, cancellationToken: cancellationtoken);
    }
}