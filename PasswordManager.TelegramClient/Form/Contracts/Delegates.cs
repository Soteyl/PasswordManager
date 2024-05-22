namespace PasswordManager.TelegramClient.Form.Contracts;

public delegate Task OnCompleteDelegate(OnCompleteFormEventArgs eventArgs, CancellationToken cancellationToken = default);

public delegate FormValidateResult ValidateFormDelegate(ValidateAnswerEventArgs eventArgs, CancellationToken cancellationToken = default);

public delegate FormStep BuildFormStepDelegate(BuildFormStepEventArgs stepEventArgs);