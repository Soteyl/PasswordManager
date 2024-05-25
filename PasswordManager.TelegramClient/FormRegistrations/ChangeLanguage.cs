using System.Globalization;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Form;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.FormRegistrations;

public class ChangeLanguage(IUserDataRepository userDataRepository): IFormRegistration
{
    private const string LanguageKey = "Language";
    
    public FormModel ResolveForm()
    {
        return new FormBuilder()
               .AddStep(s => s.Builder
                              .WithQuestion(MessageBodies.LanguageSelection)
                              .WithAnswerRow(MessageButtons.English, MessageButtons.Ukrainian)
                              .WithAnswerRow(MessageButtons.Return)
                              .WithAnswerKey(LanguageKey)
                              .ValidateAnswer(ValidateLocale))
               .OnComplete(SelectLanguage)
               .Build();
    }

    private FormValidateResult ValidateLocale(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var locale = LocaleExtensions.ToLocale(eventArgs.Answer);

        if (!locale.HasValue)
            return new FormValidateResult()
            {
                IsSuccess = false,
                Error = MessageBodies.WrongLocaleChosen
            };

        return new FormValidateResult()
        {
            IsSuccess = true,
            ValidResult = locale.Value.ToString()
        };
    }

    private async Task SelectLanguage(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var locale = Enum.Parse<Locale>(eventArgs.Data[LanguageKey], false);
        await userDataRepository.ChangeLocaleAsync(eventArgs.UserData.TelegramUserId, locale, cancellationToken);
        CultureInfo.CurrentUICulture = locale.ToCulture();
        await eventArgs.FormMessageHandler.StartFormRequestAsync<MainMenu>(eventArgs.UserData.TelegramUserId, eventArgs.ChatId, cancellationToken);
    }
}