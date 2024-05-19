using PasswordManager.TelegramClient.Commands;

namespace PasswordManager.TelegramClient.Form;

public class FormStep
{
    private readonly List<List<string>> _answers = new();
    
    public Func<Dictionary<string, string>, Task<string>> Question { get; set; }
    
    public Type CustomCommand { get; private set; }

    public IEnumerable<IEnumerable<string>>? Answers => _answers;
    
    public bool IsDeleteAnswer { get; private set; }
    
    public TimeSpan? TimeBeforeQuestionDeletion { get; private set; }
    
    public bool IsDeleteQuestionAfterAnswer { get; private set; }
    
    public bool IsWithoutAnswer { get; private set; }

    public string AnswerKey { get; private set; }
    
    public ValidateFormDelegate? Validator { get; private set; }

    public FormStep WithQuestion(string question)
    {
        Question = (_) => Task.FromResult(question);
        return this;
    }
    
    public FormStep WithQuestion(Func<Dictionary<string, string>, Task<string>> question)
    {
        Question = question;
        return this;
    }

    public FormStep WithCustomCommand<T>()
        where T: ITelegramCommand
    {
        CustomCommand = typeof(T);
        return this;
    }
    
    public FormStep WithAnswers (IEnumerable<IEnumerable<string>> answers)
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
}