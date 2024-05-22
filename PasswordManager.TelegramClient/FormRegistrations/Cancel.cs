using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class Cancel: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .RegisterCommands(MessageButtons.Cancel, MessageButtons.Return)
               .OnComplete(async (args, ct)
                   =>
               {
                   if (args.UserData.MasterPasswordHash is null)
                   {
                       await args.FormMessageHandler.StartFormRequestAsync<SetUpMasterPassword>(args.UserData.TelegramUserId,
                           args.ChatId, ct);
                       return;
                   }
                   await args.FormMessageHandler.StartFormRequestAsync<MainMenu>(args.UserData.TelegramUserId,
                       args.ChatId, ct);
               })
               .Build();
    }
}