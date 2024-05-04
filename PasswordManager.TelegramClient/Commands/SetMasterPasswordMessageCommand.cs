using PasswordManager.TelegramClient.Commands.Contracts;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands;

public class SetMasterPasswordMessageCommand : MessageCommand
{
    private readonly IUserDataRepository _userDataRepository;
    private readonly ITelegramCommandResolver _commandResolver;

    public SetMasterPasswordMessageCommand(IUserDataRepository userDataRepository, ITelegramCommandResolver commandResolver) 
        : base(userDataRepository)
    {
        _userDataRepository = userDataRepository;
        _commandResolver = commandResolver;
        MasterPasswordNeeded = false;
    }
    
    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        var password = request.Message!.Text;

        if (string.IsNullOrWhiteSpace(password))
        {
            await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.YouNeedAtLeastOneCharForPassword, cancellationToken: cancellationToken);
            return new ExecuteTelegramCommandResult()
            {
                NextListener = GetType()
            };
        }
        
        await _userDataRepository.ChangeMasterPasswordAsync(request.Message.Chat.Id, password, cancellationToken);
        
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.YourMasterPasswordIsApplied, cancellationToken: cancellationToken);

        var mainMenu = await _commandResolver.ResolveCommandAsync<MainMenuMessageCommand>(cancellationToken);
        await mainMenu.ExecuteAsync(request.Message, request.Client, cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}