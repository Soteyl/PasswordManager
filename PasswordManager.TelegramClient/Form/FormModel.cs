using PasswordManager.TelegramClient.Form.Contracts;

namespace PasswordManager.TelegramClient.Form;

public class FormModel(IReadOnlyCollection<FormStep> steps, OnCompleteDelegate? onComplete, IEnumerable<string> commands)
{
    public IReadOnlyCollection<FormStep> Steps { get; } = steps;
    public OnCompleteDelegate? OnComplete { get; } = onComplete;

    public IEnumerable<string> Commands { get; } = commands;
}