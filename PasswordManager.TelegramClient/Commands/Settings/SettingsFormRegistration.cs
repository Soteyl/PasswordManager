using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.Settings;

public class SettingsFormRegistration: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .AddStep(s => s.Builder
                              .WithQuestion(MessageButtons.Settings) // todo
                              .WithAnswerRow(MessageButtons.ChangeLanguage)
                              .WithAnswerRow(MessageButtons.Return)
                              .ExecuteAnotherForm<ChangeMasterPasswordFormRegistration>(MessageButtons.ChangeLanguage))
               .Build();
    }
}