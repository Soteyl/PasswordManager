using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class About: IFormRegistration
{
    public FormModel ResolveForm()
        => new FormBuilder()
            .AddStep(s => s.Builder
                .WithQuestion(MessageBodies.About)
                .DisableWebPagePreview()
                .WithAnswerRow(MessageButtons.Return))
            .Build();
}