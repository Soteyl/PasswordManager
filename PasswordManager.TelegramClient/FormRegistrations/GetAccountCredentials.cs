using Newtonsoft.Json;
using PasswordManager.TelegramClient.Common.Cryptography;
using PasswordManager.TelegramClient.Common.Keyboard;
using PasswordManager.TelegramClient.Common.Validation;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class GetAccountCredentials(PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
{
    private const string Account = "account";
    private const string MasterPassword = "masterPassword";
    
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
                        .WithAnswers(accountsMarkup.Select(x => new []{x}))
                        .ValidateAnswer((args, ct) => Validators.Account(args, passwordStorageService, ct))
                        .WithAnswerKey(Account);
            })
            .AddStep(s =>
            {
                var account = JsonConvert.DeserializeObject<AccountInfo>(s.Data[Account])!;
                return s.Builder
                        .WithQuestion(MessageBodiesParametrized.GetCredentialsProvideMasterPassword(account.Url, 
                            account.WebsiteNickname, account.User))
                        .WithAnswerKey(MasterPassword)
                        .DeleteAnswerMessage()
                        .WithAnswerRow(MessageButtons.Cancel)
                        .ValidateAnswer(Validators.MasterPassword);
            })
            .OnComplete(OnComplete).Build();
    }

    private async Task OnComplete(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var account = JsonConvert.DeserializeObject<AccountInfo>(eventArgs.Answers[Account]);
        var answers = new KeyboardBuilder().Return().Build();
        
        var creds = await passwordStorageService.GetAccountCredentialsAsync(new GetAccountCredentialsRequest()
        {
            AccountId = account.AccountId,
            UserId = eventArgs.UserData.InternalId.ToString()
        }, cancellationToken: cancellationToken);

        if (!creds.Response.IsSuccess)
        {
            await eventArgs.Client.SendMessageAsync(MessageBodies.InternalError, 
                eventArgs.ChatId, answers: answers, cancellationToken: cancellationToken);
        }

        var decryptedPassword = Cryptographer.Decrypt(creds.CredentialsHash.ToByteArray(), 
            creds.CredentialsSalt.ToByteArray(), 
            eventArgs.Answers[MasterPassword]);

        await eventArgs.Client.SendMessageAsync(MessageBodies.HereIsYourPassword, eventArgs.ChatId, cancellationToken: cancellationToken);

        var passwordMessage = await eventArgs.Client.SendMessageAsync(decryptedPassword, eventArgs.ChatId,
            answers: answers, cancellationToken: cancellationToken);
        
        await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        
        await eventArgs.Client.DeleteMessageAsync(passwordMessage.MessageId, eventArgs.ChatId,
            cancellationToken: cancellationToken);
    }
}