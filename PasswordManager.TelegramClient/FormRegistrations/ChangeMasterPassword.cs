using Google.Protobuf;
using PasswordManager.TelegramClient.Common.Cryptography;
using PasswordManager.TelegramClient.Common.Keyboard;
using PasswordManager.TelegramClient.Common.Validation;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class ChangeMasterPassword(IUserDataRepository userDataRepository, 
    PasswordStorageService.PasswordStorageServiceClient passwordStorageService): IFormRegistration
{
    private const string MasterPassword = "masterPassword";
    
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .RegisterCommands(MessageButtons.ChangeMasterPassword)
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.ChangeMasterPasswordConditions)
                              .WithAnswerRow(MessageButtons.Cancel)
                              .WithAnswerKey(MasterPassword)
                              .DeleteAnswerMessage()
                              .ValidateAnswer(ValidateChangeMasterPassword))
               .OnComplete(CompleteChangeMasterPassword)
               .Build();
    }

    private async Task CompleteChangeMasterPassword(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var passwords = eventArgs.Data[MasterPassword].Split(Environment.NewLine);
        var oldPassword = passwords[0];
        var newPassword = passwords[1];

        await userDataRepository.ChangeMasterPasswordAsync(eventArgs.UserData.TelegramUserId, newPassword, cancellationToken);

        var accounts = await passwordStorageService.GetAccountsWithCredentialsAsync(new GetAccountsRequest()
        {
            Limit = 1000,
            UserId = eventArgs.UserData.InternalId.ToString()
        }, cancellationToken: cancellationToken);

        if (!accounts.Response.IsSuccess)
            await eventArgs.Client.SendMessageAsync(MessageBodies.InternalError, eventArgs.ChatId,
                new KeyboardBuilder().Return().Build(), cancellationToken: cancellationToken);


        await passwordStorageService.ChangeManyAccountCredentialsAsync(new ChangeManyAccountCredentialsCommand()
        {
            UserId = eventArgs.UserData.InternalId.ToString(),
            Changes =
            {
                accounts.Accounts.Select(x =>
                {
                    var password = Cryptographer.Encrypt(
                        Cryptographer.Decrypt(x.CredentialsHash.ToByteArray(), x.CredentialsSalt.ToByteArray(), oldPassword),
                        newPassword);

                    return new ChangeAccountCredentialsCommand()
                    {
                        AccountId = x.AccountId,
                        CredentialsHash = ByteString.CopyFrom(password.CipherText),
                        CredentialsSalt = ByteString.CopyFrom(password.IV)
                    };
                })
            }
        }, cancellationToken: cancellationToken);
        
        await eventArgs.Client.SendMessageAsync(MessageBodies.MasterPasswordChanged, eventArgs.ChatId,
            new KeyboardBuilder().Return().Build(), cancellationToken: cancellationToken);
    }

    private FormValidateResult ValidateChangeMasterPassword(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var passwords = eventArgs.Answer.Split(Environment.NewLine);
        if (passwords.Length != 3)
        {
            return new FormValidateResult()
            {
                IsSuccess = false,
                Error = MessageBodies.WrongLinesCount,
                ValidResult = null
            };
        }

        var masterPasswordValidation = Validators.MasterPassword(new ValidateAnswerEventArgs()
        {
            UserData = eventArgs.UserData,
            Answer = passwords[0],
            Context = eventArgs.Context
        });
        if (!masterPasswordValidation.IsSuccess) return masterPasswordValidation;
        
        if (passwords[1] != passwords[2])
        {
            return new FormValidateResult()
            {
                IsSuccess = false,
                Error = MessageBodies.NewMasterPasswordNotMatch,
                ValidResult = null
            };
        }
        
        return new FormValidateResult()
        {
            IsSuccess = true,
            Error = string.Empty
        };
    }
}