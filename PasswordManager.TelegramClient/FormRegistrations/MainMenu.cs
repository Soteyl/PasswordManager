using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class MainMenu: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.WrongMessageWarningBody)
                              .WithAnswerRow(MessageButtons.ShowMyAccounts)
                              .WithAnswerRow(MessageButtons.Settings)
                              .ExecuteAnotherForm<GetAccounts>(MessageButtons.ShowMyAccounts)
                              .ExecuteAnotherForm<Settings>(MessageButtons.Settings))
               .Build();
    }
}