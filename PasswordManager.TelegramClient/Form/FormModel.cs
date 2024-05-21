namespace PasswordManager.TelegramClient.Form;

public class FormModel(IReadOnlyCollection<FormStep> steps, OnCompleteDelegate onComplete)
{
    public IReadOnlyCollection<FormStep> Steps { get; } = steps;
    public OnCompleteDelegate OnComplete { get; } = onComplete;
}