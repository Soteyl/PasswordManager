using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PasswordManager.TelegramClient.Common.Cryptography;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Common.Validation;

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
        var number = int.Parse(eventArgs.Answer.Split(".")[0]);
        var accountsResponse = passwordStorageService.GetAccountsAsync(new GetAccountsRequest()
        {
            UserId = eventArgs.UserData.InternalId.ToString(),
            Limit = 1,
            Skip = number - 1
        }, cancellationToken: cancellationToken).ResponseAsync.Result;
        var account = accountsResponse.Accounts.FirstOrDefault();
        bool isSuccess = accountsResponse.Response.IsSuccess && account != null;
        return new FormValidateResult()
        {
            IsSuccess = isSuccess,
            Error = isSuccess ? null : MessageBodies.InternalError,
            ValidResult = isSuccess ? JsonConvert.SerializeObject(account) : null
        };
    }
    
    public static FormValidateResult Url(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken = default)
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