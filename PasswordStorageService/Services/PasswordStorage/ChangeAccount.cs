using FluentValidation;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<ServiceResponse> ChangeAccount(ChangeAccountCommand request, ServerCallContext context)
    {
        var validation = await new ChangeAccountCommandValidator().ValidateAsync(request, context.CancellationToken);
        if (!validation.IsValid)
            return ServiceResponses.ValidationFailure(validation);
        
        var accountId = Guid.Parse(request.AccountId);
        var userId = Guid.Parse(request.UserId);
        
        var existingAccount = await database.Accounts.FirstOrDefaultAsync(x => x.Id.Equals(accountId) && x.UserId.Equals(userId), 
            context.CancellationToken);

        if (existingAccount is null)
            return ServiceResponses.NotFound(nameof(request.AccountId));
        
        existingAccount.User = request.Username;
        existingAccount.WebsiteUrl = new Uri(request.Url);
        existingAccount.WebsiteNickName = request.WebsiteNickname;
        
        await database.SaveChangesAsync(context.CancellationToken);
        return ServiceResponses.Success;
    }

    private class ChangeAccountCommandValidator: AbstractValidator<ChangeAccountCommand>
    {
        public ChangeAccountCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .IsGuid();
            
            RuleFor(x => x.UserId)
                .IsGuid();
            
            RuleFor(x => x.Url)
                .IsUri();

            RuleFor(x => x.Username)
                .NotEmpty();

            RuleFor(x => x.WebsiteNickname)
                .NotEmpty();
        }
    }
}