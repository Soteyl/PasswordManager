namespace PasswordManager.TelegramClient.Form;

public class FormBuilder
{
    private List<FormStep> _steps = new();
    private OnCompleteDelegate _onComplete;

    public FormBuilder AddStep(Func<FormStep, FormStep> step)
    {
        _steps.Add(step(new FormStep()));
        return this;
    }

    public FormBuilder OnComplete(OnCompleteDelegate onComplete)
    {
        _onComplete = onComplete;
        return this;
    }

    public FormModel Build()
    {
        return new FormModel(_steps, _onComplete);
    }
}