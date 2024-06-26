using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class MainMenu: IFormRegistration
{
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .RegisterCommands(MessageButtons.Cancel, MessageButtons.Return)
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.WrongMessageWarningBody)
                              .WithAnswerRow(MessageButtons.ShowMyAccounts)
                              .WithAnswerRow(MessageButtons.Settings, MessageButtons.About)
                              .OnlyButtonAnswer()
                              .ExecuteAnotherForm<About>(MessageButtons.About)
                              .ExecuteAnotherForm<GetAccounts>(MessageButtons.ShowMyAccounts)
                              .ExecuteAnotherForm<Settings>(MessageButtons.Settings))
               .Build();
    }
}