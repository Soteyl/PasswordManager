﻿using Google.Protobuf;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Keyboard;
using PasswordManager.TelegramClient.Resources;
using PasswordManager.TelegramClient.Validation;
using Telegram.Bot;

namespace PasswordManager.TelegramClient.Commands.AddAccount;

public class AddAccountFormRegistration(PasswordStorageService.PasswordStorageServiceClient storageService): IFormRegistration
{
    private const string Url = "url";
    private const string WebsiteNickname = "websiteNickname";
    private const string Username = "username";
    private const string Password = "password";
    private const string MasterPassword = "masterPassword";
    
    public FormModel ResolveForm()
    {
        var cancelKeyboard = new KeyboardBuilder().Cancel().Build();
        return new FormBuilder()
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodies.SendUrlToAddAccount)
                .WithAnswerKey(Url)
                .ValidateAnswer(IsValidUrl)
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
                .WithQuestion(MessageBodies.AddAccountFinalStep)
                .WithAnswerKey(MasterPassword)
                .ValidateAnswer(MasterPasswordValidation.IsValidMasterPassword)
                .DeleteQuestionAfterAnswer()
                .DeleteAnswerMessage())
            .OnComplete(OnComplete)
            .Build();
    }
    
    private async Task OnComplete(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        var result = Cryptographer.Encrypt(eventArgs.Answers[Password], eventArgs.Answers[MasterPassword]);
        
        var response = await storageService.AddAccountAsync(new AddAccountCommand()
        {
            CredentialsHash = ByteString.CopyFrom(result.CipherText),
            CredentialsSalt = ByteString.CopyFrom(result.IV),
            Url = eventArgs.Answers[Url],
            User = eventArgs.Answers[Username],
            UserId = eventArgs.UserData.InternalId.ToString(),
            WebsiteNickname = eventArgs.Answers[WebsiteNickname]
        }, cancellationToken: cancellationToken);
        
        await eventArgs.Client.SendTextMessageAsync(eventArgs.ChatId,
            (response.Response.IsSuccess) ? MessageBodies.AddAccountSuccess : MessageBodies.InternalError, 
            replyMarkup: KeyboardBuilder.GetMarkup(new KeyboardBuilder().Return().Build()), cancellationToken: cancellationToken);
    }
    
    private static FormValidateResult IsValidUrl(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        if (!eventArgs.Answer.Contains("http")) eventArgs.Answer = "https://" + eventArgs.Answer;
        Uri.TryCreate(eventArgs.Answer, UriKind.Absolute, out Uri? validatedUri);
        var validUrl = validatedUri?.ToString() ?? string.Empty;
        var isValid = validatedUri != null && (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);

        return new FormValidateResult()
        {
            IsSuccess = isValid,
            Error = isValid ? string.Empty : MessageBodies.WrongUrlFormat,
            ValidResult = isValid ? validUrl : null
        };
    }
}