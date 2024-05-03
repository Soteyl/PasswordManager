using FluentValidation;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<GetAccountsResponse> GetAccounts(GetAccountsRequest request, ServerCallContext context)
    {
        var validation = new GetAccountsRequestValidator().Validate(request);
        if (!validation.IsValid)
            return new GetAccountsResponse() { Response = ServiceResponses.ValidationFailure(validation) };

        var userId = Guid.Parse(request.UserId);
        
        var accounts = await database.Accounts
            .Where(x => x.UserId.Equals(userId))
            .Skip(request.Skip)
            .Take(request.Limit)
            .Select(x => new AccountInfo()
            {
                AccountId = x.Id.ToString(),
                User = x.User,
                WebsiteNickname = x.WebsiteNickName,
                Url = x.WebsiteUrl.ToString()
            })
            .ToListAsync(context.CancellationToken);

        return new GetAccountsResponse()
        {
            Response = ServiceResponses.Success,
            Accounts = { accounts }
        };
    }
    
    private class GetAccountsRequestValidator : AbstractValidator<GetAccountsRequest>
    {
        public GetAccountsRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .IsGuid();

            RuleFor(x => x.Limit)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0);
        }
    }
}