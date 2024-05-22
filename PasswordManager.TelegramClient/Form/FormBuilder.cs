using PasswordManager.TelegramClient.Form.Contracts;

namespace PasswordManager.TelegramClient.Form;

public class FormBuilder
{
    private List<FormStep> _steps = new();
    private OnCompleteDelegate _onComplete;
    private List<string> _commands = new();

    public FormBuilder RegisterCommands(params string[] commands)
    {
        _commands.AddRange(commands);
        return this;
    }

    public FormBuilder AddStep(BuildFormStepDelegate step)
    {
        _steps.Add(new FormStep(step));
        return this;
    }

    public FormBuilder OnComplete(OnCompleteDelegate onComplete)
    {
        _onComplete = onComplete;
        return this;
    }

    public FormModel Build()
    {
        return new FormModel(_steps, _onComplete, _commands);
    }
}