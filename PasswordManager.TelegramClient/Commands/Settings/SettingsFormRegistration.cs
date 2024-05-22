using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands.Settings;

public class SettingsFormRegistration: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .RegisterCommands(MessageButtons.Settings)
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.ChooseSettingsToUpdate)
                              .WithAnswerRow(MessageButtons.ChangeMasterPassword)
                              .WithAnswerRow(MessageButtons.Return)
                              .ExecuteAnotherForm<ChangeMasterPasswordFormRegistration>(MessageButtons.ChangeMasterPassword))
               .Build();
    }
}