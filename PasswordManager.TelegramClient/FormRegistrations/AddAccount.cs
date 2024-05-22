using Google.Protobuf;
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
                .WithQuestion(MessageBodiesParametrized.AddAccountFinalStep(
                    s.Data[WebsiteNickname], s.Data[Url], s.Data[Username], s.Data[Password]))
                .WithAnswerKey(MasterPassword)
                .ValidateAnswer(Validators.MasterPassword)
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
        
        await eventArgs.Client.SendMessageAsync(
            (response.Response.IsSuccess) ? MessageBodies.AddAccountSuccess : MessageBodies.InternalError,
            eventArgs.ChatId,
            answers: new KeyboardBuilder().Return().Build(), cancellationToken: cancellationToken);
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