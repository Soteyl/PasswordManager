using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.TelegramClient.Commands.SetUpMasterPassword;

public class SetUpMasterPasswordFormRegistration(
    IUserDataRepository userDataRepository, ITelegramCommandResolver commandResolver): IFormRegistration
{
    private const string MasterPassword = "masterPassword";
    
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .AddStep(s => s
                             .WithQuestion(MessageBodies.SetUpMasterPasswordMessageBody)
                             .DeleteAnswerMessage()
                             .WithAnswerKey(MasterPassword))
               .OnComplete(OnComplete)
               .Build();
    }

    private async Task OnComplete(OnCompleteFormEventArgs args, CancellationToken cancellationToken)
    {
        await userDataRepository.ChangeMasterPasswordAsync(args.ChatId, args.Answers[MasterPassword], cancellationToken);
        
        await args.Client.SendTextMessageAsync(args.ChatId, MessageBodies.YourMasterPasswordIsApplied, cancellationToken: cancellationToken);

        var mainMenu = await commandResolver.ResolveCommandAsync<MainMenuMessageCommand>(cancellationToken);
        await mainMenu.ExecuteAsync(new Message()
        {
            Chat = new Chat()
            {
                Id = args.ChatId
            },
            From = new User()
            {
                Id = args.ChatId
            }
        }, args.Client, cancellationToken);
    }
}