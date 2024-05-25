using Google.Protobuf;
using Newtonsoft.Json;
using PasswordManager.TelegramClient.Common.Cryptography;
using PasswordManager.TelegramClient.Common.Keyboard;
using PasswordManager.TelegramClient.Common.Validation;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class ChangeAccount(PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
{
    private const string Account = "account";
    private const string ChangeAction = "changeAction";
    private const string Answer = "answer";
    private const string MasterPassword = "masterPassword";
    private readonly Dictionary<string, string> _questionByAnswer = new()
    {
        { MessageButtons.WebsiteNickname, MessageBodies.ChangeWebsiteNickname },
        { MessageButtons.WebsiteUrl, MessageBodies.ChangeWebsiteUrl },
        { MessageButtons.Username, MessageBodies.ChangeUsername },
        { MessageButtons.Password, MessageBodies.ChangePassword }
    };

    public FormModel ResolveForm()
        => new FormBuilder()
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
                       .WithAnswers(accountsMarkup.Select(x => new[]
                       {
                           x
                       }))
                       .OnlyButtonAnswer()
                       .ValidateAnswer((args, ct) => Validators.Account(args, passwordStorageService, ct))
                       .WithAnswerKey(Account);
           })
           .AddStep(s => s.Builder
                          .WithQuestion(
                              MessageBodiesParametrized.ChooseWhatChangeInAccount(
                                  JsonConvert.DeserializeObject<AccountInfo>(s.Data[Account])!))
                          .WithAnswerRow(MessageButtons.WebsiteNickname, 
                              MessageButtons.WebsiteUrl)
                          .WithAnswerRow(MessageButtons.Username, 
                              MessageButtons.Password)
                          .WithAnswerRow(MessageButtons.Cancel)
                          .OnlyButtonAnswer()
                          .WithAnswerKey(ChangeAction))
           .AddStep(s =>
           {
               var changeAction = s.Data[ChangeAction];
               var builder = s.Builder
                              .WithQuestion(_questionByAnswer[changeAction])
                              .WithAnswerKey(Answer)
                              .WithAnswerRow(MessageButtons.Cancel);

               if (changeAction == MessageButtons.WebsiteUrl)
                   builder = builder.ValidateAnswer(Validators.Url);

               if (changeAction == MessageButtons.Password)
                   builder = builder.DeleteAnswerMessage();
               
               return builder;
           })
           .AddStep(s => s.Builder
                          .ConditionalStep(() => s.Data[ChangeAction] == MessageButtons.Password)
                          .WithQuestion(MessageBodies.SendMasterPasswordToEncrypt)
                          .ValidateAnswer(Validators.MasterPassword)
                          .WithAnswerRow(MessageButtons.Cancel)
                          .WithAnswerKey(MasterPassword)
                          .DeleteAnswerMessage())
           .OnComplete(CompleteChangingAccount)
           .Build();

    private async Task CompleteChangingAccount(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var account = JsonConvert.DeserializeObject<AccountInfo>(eventArgs.Data[Account])!;
        var action = eventArgs.Data[ChangeAction];
        var answer = eventArgs.Data[Answer];
        
        var response = action == MessageButtons.Password 
            ? await ChangePasswordAsync(account, eventArgs, cancellationToken)
            : await passwordStorageService.ChangeAccountAsync(new ChangeAccountCommand()
            {
                WebsiteNickname = action == MessageButtons.WebsiteNickname ? answer : account.WebsiteNickname,
                Url = action == MessageButtons.WebsiteUrl ? answer : account.Url,
                Username = action == MessageButtons.Username ? answer : account.User,
                AccountId = account.AccountId,
                UserId = eventArgs.UserData.InternalId.ToString()
            }, cancellationToken: cancellationToken);

        await eventArgs.Client.SendMessageAsync(response.IsSuccess
                ? MessageBodies.AccountChanged
                : MessageBodies.InternalError,
            eventArgs.ChatId,
            answers: new KeyboardBuilder().Return().Build(),
            cancellationToken: cancellationToken);
    }

    private async Task<ServiceResponse> ChangePasswordAsync(AccountInfo accountInfo, OnCompleteFormEventArgs eventArgs,
        CancellationToken cancellationToken = default)
    {
        var userId = eventArgs.UserData.InternalId.ToString();
        var result = await passwordStorageService.GetAccountCredentialsAsync(new GetAccountCredentialsRequest()
        {
            AccountId = accountInfo.AccountId,
            UserId = userId
        }, cancellationToken: cancellationToken);

        if (!result.Response.IsSuccess)
            return result.Response;

        var password = Cryptographer.Encrypt(eventArgs.Data[Answer], eventArgs.Data[MasterPassword]);

        var response = await passwordStorageService.ChangeManyAccountCredentialsAsync(new ChangeManyAccountCredentialsCommand()
        {
            UserId = userId,
            Changes = { new ChangeAccountCredentialsCommand()
            {
                AccountId = accountInfo.AccountId,
                CredentialsHash = ByteString.CopyFrom(password.CipherText),
                CredentialsSalt = ByteString.CopyFrom(password.IV)
            } }
        }, cancellationToken: cancellationToken);

        return response;
    }
}