using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class Settings: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .RegisterCommands(MessageButtons.Settings)
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.ChooseSettingsToUpdate)
                              .WithAnswerRow(MessageButtons.ChangeMasterPassword)
                              .WithAnswerRow(MessageButtons.ChangeLanguage)
                              .WithAnswerRow(MessageButtons.Return)
                              .ExecuteAnotherForm<ChangeMasterPassword>(MessageButtons.ChangeMasterPassword)
                              .ExecuteAnotherForm<ChangeLanguage>(MessageButtons.ChangeLanguage))
               .Build();
    }
}