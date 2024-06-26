﻿using Google.Protobuf;
using Newtonsoft.Json;
using PasswordManager.TelegramClient.Common.Cryptography;
using PasswordManager.TelegramClient.Common.Keyboard;
using PasswordManager.TelegramClient.Common.Validation;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class AddAccount(PasswordStorageService.PasswordStorageServiceClient storageService): IFormRegistration
{
    private const string Url = "url";
    private const string WebsiteNickname = "websiteNickname";
    private const string Username = "username";
    private const string Password = "password";
    private const string EncryptedPassword = "masterPassword";
    
    public FormModel ResolveForm()
    {
        var cancelKeyboard = new KeyboardBuilder().Cancel().Build();
        return new FormBuilder()
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodies.SendUrlToAddAccount)
                .WithAnswerKey(Url)
                .ValidateAnswer(Validators.Url)
                .WithAnswers(cancelKeyboard))
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodies.SendWebsiteNicknameToAddAccount)
                .WithAnswerKey(WebsiteNickname)
                .WithAnswers(cancelKeyboard))
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodies.SendUserToAddAccount)
                .WithAnswerKey(Username)
                .WithAnswers(cancelKeyboard))
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodies.SendPasswordToAddAccount)
                .WithAnswerKey(Password)
                .WithAnswers(cancelKeyboard)
                .DeleteAnswerMessage())
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodiesParametrized.AddAccountFinalStep(
                    s.Data[WebsiteNickname], s.Data[Url], s.Data[Username], s.Data[Password]))
                .WithAnswerRow(MessageButtons.Cancel)
                .WithAnswerKey(EncryptedPassword)
                .ValidateAnswer((args, ct) =>
                {
                    var validation = Validators.MasterPassword(args, ct);
                    if (!validation.IsSuccess) return validation;

                    return new FormValidateResult()
                    {
                        ValidResult = JsonConvert.SerializeObject(Cryptographer.Encrypt(s.Data[Password], args.Answer)),
                        IsSuccess = true
                    };
                })
                .DeleteQuestionAfterAnswer()
                .DeleteAnswerMessage())
            .OnComplete(OnComplete)
            .Build();
    }
    
    private async Task OnComplete(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        var result = JsonConvert.DeserializeObject<EncryptResult>(eventArgs.Data[EncryptedPassword])!;
        
        var response = await storageService.AddAccountAsync(new AddAccountCommand()
        {
            CredentialsHash = ByteString.CopyFrom(result.CipherText),
            CredentialsSalt = ByteString.CopyFrom(result.IV),
            Url = eventArgs.Data[Url],
            User = eventArgs.Data[Username],
            UserId = eventArgs.UserData.InternalId.ToString(),
            WebsiteNickname = eventArgs.Data[WebsiteNickname]
        }, cancellationToken: cancellationToken);
        
        await eventArgs.Client.SendMessageAsync(
            (response.Response.IsSuccess) ? MessageBodies.AddAccountSuccess : MessageBodies.InternalError,
            eventArgs.ChatId,
            answers: new KeyboardBuilder().Return().Build(), cancellationToken: cancellationToken);
    }
}