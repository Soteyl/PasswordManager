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
    private const string Password = "masterPassword";
    
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
                
                var accountsMarkup = accounts.Accounts.Select((x, i) => $"{i + 1}. {x.WebsiteNickname} ({x.User})").ToList();
                accountsMarkup.Add(MessageButtons.Cancel);
                
                return s.Builder
                        .WithQuestion(message)
                        .WithAnswers(accountsMarkup.Select(x => new []{x}))
                        .OnlyButtonAnswer()
                        .ValidateAnswer((args, ct) => Validators.Account(args, passwordStorageService, ct))
                        .WithAnswerKey(Account);
            })
            .AddStep(s =>
            {
                var account = JsonConvert.DeserializeObject<AccountInfo>(s.Data[Account])!;
                return s.Builder
                        .WithQuestion(MessageBodiesParametrized.GetCredentialsProvideMasterPassword(account.Url, 
                            account.WebsiteNickname, account.User))
                        .WithAnswerKey(Password)
                        .DeleteAnswerMessage()
                        .WithAnswerRow(MessageButtons.Cancel)
                        .ValidateAnswer((args, ct) =>
                        {
                            var validation = Validators.MasterPassword(args, ct);
                            if (!validation.IsSuccess) return validation;
                            
                            var creds = passwordStorageService.GetAccountCredentialsAsync(new GetAccountCredentialsRequest()
                            {
                                AccountId = account.AccountId,
                                UserId = args.UserData.InternalId.ToString()
                            }, cancellationToken: ct).ResponseAsync.Result;

                            if (!creds.Response.IsSuccess)
                            {
                                return new FormValidateResult()
                                {
                                    IsSuccess = false,
                                    Error = MessageBodies.InternalError
                                };
                            }
                            
                            var decryptedPassword = Cryptographer.Decrypt(creds.CredentialsHash.ToByteArray(), 
                                creds.CredentialsSalt.ToByteArray(), 
                                args.Answer);

                            return new FormValidateResult()
                            {
                                ValidResult = decryptedPassword,
                                IsSuccess = true
                            };
                        });
            })
            .OnComplete(OnComplete).Build();
    }

    private async Task OnComplete(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var answers = new KeyboardBuilder().Return().Build();

        await eventArgs.Client.SendMessageAsync(MessageBodies.HereIsYourPassword, 
            eventArgs.ChatId, 
            answers: new KeyboardBuilder().Return().Build(), 
            cancellationToken: cancellationToken);

        var passwordMessage = await eventArgs.Client.SendMessageAsync(eventArgs.Data[Password], eventArgs.ChatId,
            answers: answers, cancellationToken: cancellationToken);
        
        await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        
        await eventArgs.Client.DeleteMessageAsync(passwordMessage.MessageId, eventArgs.ChatId,
            cancellationToken: cancellationToken);
    }
}