using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands;

public class CancelFormRegistration: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .RegisterCommands(MessageButtons.Cancel, MessageButtons.Return)
               .OnComplete(async (args, ct)
                   => await args.FormMessageHandler.StartFormRequestAsync<MainMenuFormRegistration>(args.UserData.TelegramUserId,
                       args.ChatId, ct))
               .Build();
    }
}