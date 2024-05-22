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
                   => await args.FormMessageHandler.StartFormRequestAsync<MainMenu>(args.UserData.TelegramUserId,
                       args.ChatId, ct))
               .Build();
    }
}