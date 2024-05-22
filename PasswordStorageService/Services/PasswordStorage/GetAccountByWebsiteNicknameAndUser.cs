using FluentValidation;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<GetAccountByWebsiteNicknameAndUserResult> GetAccountByWebsiteNicknameAndUser(GetAccountByWebsiteNicknameAndUserRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var account = await database.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId.Equals(userId)
                                              && x.WebsiteNickName.Equals(request.WebsiteNickname)
                                              && x.User.Equals(request.AccountUser));

        if (account is null)
            return new GetAccountByWebsiteNicknameAndUserResult()
            {
                Response = ServiceResponses.NotFound(nameof(request.AccountUser))
            };

        return new GetAccountByWebsiteNicknameAndUserResult()
        {
            Response = ServiceResponses.Success,
            Account = new AccountInfo()
            {
                AccountId = account.Id.ToString(),
                WebsiteNickname = account.WebsiteNickName,
                User = account.User,
                Url = account.WebsiteUrl.ToString()
            }
        };
    }

    private class GetAccountByWebsiteNicknameAndUserRequestValidator : AbstractValidator<GetAccountByWebsiteNicknameAndUserRequest>
    {
        public GetAccountByWebsiteNicknameAndUserRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().IsGuid();
            RuleFor(x => x.WebsiteNickname).NotEmpty();
            RuleFor(x => x.AccountUser).NotEmpty();
        }
    }
}