using FluentValidation;
using Grpc.Core;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<ServiceResponse> ChangeManyAccountCredentials(ChangeManyAccountCredentialsCommand request, ServerCallContext context)
    {
        var validation = await new ChangeManyAccountCredentialsCommandValidator().ValidateAsync(request, context.CancellationToken);
        if (!validation.IsValid)
            return ServiceResponses.ValidationFailure(validation);
        
        foreach (var change in request.Changes)
        {
            var entity = await database.Accounts.FindAsync(Guid.Parse(change.AccountId), context.CancellationToken);
            if (entity is null)
                return ServiceResponses.NotFound(nameof(change.AccountId));
            entity.CredentialsHash = change.CredentialsHash.ToByteArray();
            entity.CredentialsSalt = change.CredentialsSalt.ToByteArray();
        }

        await database.SaveChangesAsync(context.CancellationToken);

        return ServiceResponses.Success;
    }

    private class ChangeManyAccountCredentialsCommandValidator: AbstractValidator<ChangeManyAccountCredentialsCommand>
    {
        public ChangeManyAccountCredentialsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleForEach(x => x.Changes)
                .SetValidator(_ => new ChangeManyAccountCredentialsChangeValidator());
        }
    }
    
    private class ChangeManyAccountCredentialsChangeValidator: AbstractValidator<ChangeAccountCredentialsCommand>
    {
        public ChangeManyAccountCredentialsChangeValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .IsGuid();
            
            RuleFor(x => x.CredentialsHash)
                .NotEmpty();
            
            RuleFor(x => x.CredentialsSalt)
                .NotEmpty();
        }
    }
}