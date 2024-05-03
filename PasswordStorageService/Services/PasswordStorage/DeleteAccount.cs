using FluentValidation;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<ServiceResponse> DeleteAccount(DeleteAccountCommand request, ServerCallContext context)
    {
        var validation = await new DeleteAccountCommandValidator().ValidateAsync(request);
        if (!validation.IsValid) return ServiceResponses.ValidationFailure(validation);
        
        var accountId = Guid.Parse(request.AccountId);
        var userId = Guid.Parse(request.UserId);

        var entity = await database.Accounts.FirstOrDefaultAsync(x => x.Id.Equals(accountId) && x.UserId.Equals(userId), context.CancellationToken);
        if (entity is null) return ServiceResponses.NotFound(nameof(request.AccountId));

        database.Remove(entity);
        await database.SaveChangesAsync(context.CancellationToken);

        return ServiceResponses.Success;
    }

    private class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
    {
        public DeleteAccountCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .IsGuid();

            RuleFor(x => x.UserId)
                .NotEmpty()
                .IsGuid();
        }
    }
}