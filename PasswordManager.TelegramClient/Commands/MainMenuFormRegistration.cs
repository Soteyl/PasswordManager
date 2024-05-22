using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Commands.Settings;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Commands;

public class MainMenuFormRegistration: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.WrongMessageWarningBody)
                              .WithAnswerRow(MessageButtons.ShowMyAccounts)
                              .WithAnswerRow(MessageButtons.Settings)
                              .ExecuteAnotherForm<GetAccountsFormRegistration>(MessageButtons.ShowMyAccounts)
                              .ExecuteAnotherForm<SettingsFormRegistration>(MessageButtons.Settings))
               .Build();
    }
}