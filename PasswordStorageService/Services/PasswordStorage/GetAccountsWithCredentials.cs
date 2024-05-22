using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PasswordManager;
using PasswordStorageService.Common;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController
{
    public override async Task<GetAccountsWithCredentialsResponse> GetAccountsWithCredentials(GetAccountsRequest request, ServerCallContext context)
    {
        var validation = await new GetAccountsRequestValidator().ValidateAsync(request, context.CancellationToken);
        if (!validation.IsValid)
            return new GetAccountsWithCredentialsResponse() { Response = ServiceResponses.ValidationFailure(validation) };

        var userId = Guid.Parse(request.UserId);
        
        var accounts = await database.Accounts
                                     .Where(x => x.UserId.Equals(userId))
                                     .Skip(request.Skip)
                                     .Take(request.Limit)
                                     .Select(x => new AccountWithCredentialsInfo()
                                     {
                                         AccountId = x.Id.ToString(),
                                         User = x.User,
                                         WebsiteNickname = x.WebsiteNickName,
                                         Url = x.WebsiteUrl.ToString(),
                                         CredentialsHash = ByteString.CopyFrom(x.CredentialsHash),
                                         CredentialsSalt = ByteString.CopyFrom(x.CredentialsSalt)
                                     })
                                     .ToListAsync(context.CancellationToken);

        return new GetAccountsWithCredentialsResponse()
        {
            Response = ServiceResponses.Success,
            Accounts = { accounts }
        };
    }
}