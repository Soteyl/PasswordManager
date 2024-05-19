namespace PasswordManager.TelegramClient.Form;

public delegate Task OnCompleteDelegate(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken = default);

public delegate FormValidateResult ValidateFormDelegate(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken = default);