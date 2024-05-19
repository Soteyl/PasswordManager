using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Validation;

public static class MasterPasswordValidation
{
    public static FormValidateResult IsValidMasterPassword(ValidateAnswerEventArgs eventArgs,
        CancellationToken cancellationToken = default)
    {
        if (eventArgs.UserData.MasterPasswordHash == Cryptographer.GetHash(eventArgs.Answer))
        {
            return new FormValidateResult()
            {
                IsSuccess = true,
                Error = string.Empty,
                ValidResult = eventArgs.Answer
            };
        }
        
        return new FormValidateResult()
        {
            IsSuccess = false,
            Error = MessageBodies.WrongMasterPassword,
            ValidResult = null
        };
    }
}