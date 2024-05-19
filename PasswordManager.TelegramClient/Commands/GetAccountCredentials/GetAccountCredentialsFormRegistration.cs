﻿using Newtonsoft.Json;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Keyboard;
using PasswordManager.TelegramClient.Resources;
using PasswordManager.TelegramClient.Validation;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.GetAccountCredentials;

public class GetAccountCredentialsFormRegistration(PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
{
    private const string Account = "account";
    private const string MasterPassword = "masterPassword";
    
    public FormModel ResolveForm()
    {
        return new FormBuilder()
            .AddStep(s => s
                .WithCustomCommand<GetAccountCredentialsChooseAccountMessageCommand>()
                .ValidateAnswer(ValidateAccount)
                .WithAnswerKey(Account))
            .AddStep(s => s
                .WithQuestion(GetMasterPasswordForAccountCredentialsQuestion)
                .WithAnswerKey(MasterPassword)
                .DeleteAnswerMessage()
                .WithAnswerRow(MessageButtons.Cancel)
                .ValidateAnswer(MasterPasswordValidation.IsValidMasterPassword))
            .OnComplete(OnComplete).Build();
    }

    private async Task OnComplete(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var account = JsonConvert.DeserializeObject<AccountInfo>(eventArgs.Answers[Account])!;
        var returnMarkup = KeyboardBuilder.GetMarkup(new KeyboardBuilder().Return().Build());
        
        var creds = await passwordStorageService.GetAccountCredentialsAsync(new GetAccountCredentialsRequest()
        {
            AccountId = account.AccountId,
            UserId = eventArgs.UserData.InternalId.ToString()
        }, cancellationToken: cancellationToken);

        if (!creds.Response.IsSuccess)
        {
            await eventArgs.Client.SendTextMessageAsync(eventArgs.ChatId, MessageBodies.InternalError, 
                replyMarkup: returnMarkup, cancellationToken: cancellationToken);
        }

        var decryptedPassword = Cryptographer.Decrypt(creds.CredentialsHash.ToByteArray(), 
            creds.CredentialsSalt.ToByteArray(), 
            eventArgs.Answers[MasterPassword]);

        await eventArgs.Client.SendTextMessageAsync(eventArgs.ChatId, MessageBodies.HereIsYourPassword, 
            cancellationToken: cancellationToken);

        var passwordMessage = await eventArgs.Client.SendTextMessageAsync(eventArgs.ChatId, decryptedPassword,
            replyMarkup: returnMarkup, cancellationToken: cancellationToken);
        
        await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        
        await eventArgs.Client.DeleteMessageAsync(eventArgs.ChatId, passwordMessage.MessageId,
            cancellationToken: cancellationToken);
    }

    private Task<string> GetMasterPasswordForAccountCredentialsQuestion(Dictionary<string, string> context)
    {
        var account = JsonConvert.DeserializeObject<AccountInfo>(context[Account])!;
        return Task.FromResult(MessageBodiesParametrized.GetCredentialsProvideMasterPassword(account.Url, 
            account.WebsiteNickname, account.User));
    }

    private FormValidateResult ValidateAccount(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var splittedMessageData = eventArgs.Answer.Split(" ");
        var account = passwordStorageService.GetAccountByWebsiteNicknameAndUser(new GetAccountByWebsiteNicknameAndUserRequest()
        {
            UserId = eventArgs.UserData.InternalId.ToString(),
            WebsiteNickname = splittedMessageData[0],
            AccountUser = splittedMessageData[1].Replace("(", "").Replace(")", "")
        });
        bool isSuccess = account.Response.IsSuccess && account.Account != null;
        return new FormValidateResult()
        {
            IsSuccess = isSuccess,
            Error = isSuccess ? null : MessageBodies.InternalError,
            ValidResult = isSuccess ? JsonConvert.SerializeObject(account.Account) : null
        };
    }
}