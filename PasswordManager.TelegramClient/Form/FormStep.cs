using PasswordManager.TelegramClient.Commands;
using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Form;

public class FormStep
{
    private readonly BuildFormStepDelegate _buildFunc;
    
    private readonly List<List<string>> _answers = new();

    public string Question { get; set; }

    public IEnumerable<IEnumerable<string>>? Answers => _answers;

    public bool IsDeleteAnswer { get; private set; }

    public TimeSpan? TimeBeforeQuestionDeletion { get; private set; }

    public bool IsDeleteQuestionAfterAnswer { get; private set; }

    public bool IsWithoutAnswer { get; private set; }

    public string AnswerKey { get; private set; }

    public ValidateFormDelegate? Validator { get; private set; }

    public FormStep(BuildFormStepDelegate buildFunc)
    {
        _buildFunc = buildFunc;
    }

    public FormStep()
    {
        
    }

    public FormStep WithQuestion(string question)
    {
        Question = question;
        return this;
    }

    public FormStep WithAnswers(IEnumerable<IEnumerable<string>> answers)
    {
        _answers.AddRange(answers.Select(x => x.ToList()));

        return this;
    }

    public FormStep WithAnswerRow(params string[] answers)
    {
        _answers.Add(answers.ToList());

        return this;
    }

    public FormStep WithAnswerKey(string answerKey)
    {
        AnswerKey = answerKey;

        return this;
    }

    public FormStep WithoutAnswer()
    {
        IsWithoutAnswer = true;

        return this;
    }

    public FormStep DeleteAnswerMessage()
    {
        IsDeleteAnswer = true;

        return this;
    }

    public FormStep DeleteQuestionAfter(TimeSpan timeSpan)
    {
        TimeBeforeQuestionDeletion = timeSpan;

        return this;
    }

    public FormStep DeleteQuestionAfterAnswer()
    {
        IsDeleteQuestionAfterAnswer = true;

        return this;
    }

    public FormStep ValidateAnswer(ValidateFormDelegate validator)
    {
        Validator = validator;

        return this;
    }

    public async Task<FormStep> BuildAsync(TelegramUserDataEntity user, Dictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        return _buildFunc(new BuildFormStepEventArgs()
        {
            UserData = user,
            Data = data,
            Builder = new FormStep()
        });
    }
}