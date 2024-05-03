using FluentValidation;
using Grpc.Core;
using PasswordManager;
using PasswordStorageService.Common;
using PasswordStorageService.Data.Entities;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<AddAccountResult> AddAccount(AddAccountCommand request, ServerCallContext context)
    {
        var validation = await new AddAccountCommandValidator().ValidateAsync(request);
        if (!validation.IsValid)
            return new AddAccountResult() { Response = ServiceResponses.ValidationFailure(validation) };

        var entry = database.Accounts.Add(new AccountEntity()
        {
            User = request.User,
            UserId = Guid.Parse(request.UserId),
            CredentialsHash = request.CredentialsHash,
            WebsiteUrl = new Uri(request.Url),
            WebsiteNickName = request.WebsiteNickname
        });

        await database.SaveChangesAsync(context.CancellationToken);

        return new AddAccountResult()
        {
            Response = ServiceResponses.Success,
            AccountId = entry.Entity.Id.ToString()
        };
    }

    private class AddAccountCommandValidator : AbstractValidator<AddAccountCommand>
    {
        public AddAccountCommandValidator()
        {
            RuleFor(x => x.CredentialsHash)
                .NotEmpty();

            RuleFor(x => x.Url)
                .NotEmpty()
                .IsUri();

            RuleFor(x => x.User)
                .NotEmpty();

            RuleFor(x => x.WebsiteNickname)
                .NotEmpty();

            RuleFor(x => x.UserId)
                .NotEmpty()
                .IsGuid();
        }
    }
}