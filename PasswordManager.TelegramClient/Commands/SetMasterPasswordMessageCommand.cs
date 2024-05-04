using PasswordManager.TelegramClient.Data.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands;

public class SetMasterPasswordMessageCommand(IUserDataRepository userDataRepository, ITelegramCommandResolver commandResolver): MessageCommand(userDataRepository, commandResolver)
{
    public override Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var password = request.Message!.Text;

        if (string.IsNullOrWhiteSpace(password))
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.YouNeedAtLeastOneCharForPassword, cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult()
            {
                NextListener = this
            };
        }
        
        await userDataRepository.ChangeMasterPasswordAsync(request.Message.Chat.Id, password, cancellationToken);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.YourMasterPasswordIsApplied, cancellationToken: cancellationToken);

        var mainMenu = await commandResolver.ResolveCommandAsync<MainMenuMessageCommand>(cancellationToken);
        await mainMenu.ExecuteAsync(request.Message, request.Client, cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}