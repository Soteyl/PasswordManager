using FluentValidation;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<GetAccountCredentialsResult> GetAccountCredentials(GetAccountCredentialsRequest request, ServerCallContext context)
    {
        var validation = await new GetAccountCredentialsRequestValidator().ValidateAsync(request, context.CancellationToken);
        if (!validation.IsValid)
            return new GetAccountCredentialsResult() { Response = ServiceResponses.ValidationFailure(validation) };

        var accountId = Guid.Parse(request.AccountId);
        var userId = Guid.Parse(request.UserId);
        
        var entity = await database.Accounts.FirstOrDefaultAsync(x => x.Id.Equals(accountId) && x.UserId.Equals(userId), context.CancellationToken);
        if (entity is null)
            return new GetAccountCredentialsResult() { Response = ServiceResponses.NotFound(nameof(request.AccountId)) };
        
        return new GetAccountCredentialsResult()
        {
            Response = ServiceResponses.Success,
            CredentialsHash = ByteString.CopyFrom(entity.CredentialsHash),
            CredentialsSalt = ByteString.CopyFrom(entity.CredentialsSalt)
        };
    }
    
    private class GetAccountCredentialsRequestValidator : AbstractValidator<GetAccountCredentialsRequest>
    {
        public GetAccountCredentialsRequestValidator()
        {
            RuleFor(x => x.AccountId)
                .IsGuid();

            RuleFor(x => x.UserId)
                .IsGuid();
        }
    }
}