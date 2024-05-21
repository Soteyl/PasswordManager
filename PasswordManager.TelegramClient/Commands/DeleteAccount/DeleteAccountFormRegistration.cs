using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Form;

namespace PasswordManager.TelegramClient.Commands.DeleteAccount;

public class DeleteAccountFormRegistration: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder().Build();
    }
}