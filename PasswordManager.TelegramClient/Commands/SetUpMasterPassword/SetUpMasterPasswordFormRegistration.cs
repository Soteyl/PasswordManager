using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.SetUpMasterPassword;

public class SetUpMasterPasswordFormRegistration(
    IUserDataRepository userDataRepository): IFormRegistration
{
    private const string MasterPassword = "masterPassword";
    
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .AddStep(s => s.Builder
                             .WithQuestion(MessageBodies.SetUpMasterPasswordMessageBody)
                             .DeleteAnswerMessage()
                             .WithAnswerKey(MasterPassword))
               .OnComplete(OnComplete)
               .Build();
    }

    private async Task OnComplete(OnCompleteFormEventArgs args, CancellationToken cancellationToken)
    {
        await userDataRepository.ChangeMasterPasswordAsync(args.ChatId, args.Answers[MasterPassword], cancellationToken);
        
        await args.Client.SendMessageAsync(MessageBodies.YourMasterPasswordIsApplied, args.ChatId, cancellationToken: cancellationToken);

        await args.FormMessageHandler.StartFormRequestAsync<MainMenuFormRegistration>(args.UserData.TelegramUserId, args.ChatId,
            cancellationToken);
    }
}