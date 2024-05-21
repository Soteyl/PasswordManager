using Newtonsoft.Json;
using PasswordManager.TelegramClient.Cryptography;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Validation;

public static class Validators
{
    public static FormValidateResult MasterPassword(ValidateAnswerEventArgs eventArgs,
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
    
    public static FormValidateResult Account(ValidateAnswerEventArgs eventArgs, 
        PasswordStorageService.PasswordStorageServiceClient passwordStorageService, CancellationToken cancellationToken)
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