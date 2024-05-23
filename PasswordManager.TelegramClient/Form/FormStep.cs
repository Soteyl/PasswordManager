using PasswordManager.TelegramClient.Data.Entities;
using PasswordManager.TelegramClient.Form.Contracts;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Form;

public class FormStep
{
    private readonly BuildFormStepDelegate _buildFunc;
    
    private readonly List<List<string>> _answers = new();
    
    private readonly Dictionary<string, Type> _nextForms = new();

    public string? Question { get; set; }

    public IEnumerable<IEnumerable<string>>? Answers => _answers;

    public bool IsDeleteAnswer { get; private set; }

    public TimeSpan? TimeBeforeQuestionDeletion { get; private set; }

    public bool IsDeleteQuestionAfterAnswer { get; private set; }
    
    public bool IsDisableWebPagePreview { get; private set; }

    public string AnswerKey { get; private set; }

    public ValidateFormDelegate? Validator { get; private set; }

    public IReadOnlyDictionary<string, Type> NextForms => _nextForms;

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

    public FormStep OnlyButtonAnswer()
    {
        Validator = (args, token) =>
        {
            if (_answers.Any(x => x.Contains(args.Answer)))
                return new FormValidateResult()
                {
                    IsSuccess = true
                };

            return new FormValidateResult()
            {
                IsSuccess = false,
                Error = MessageBodies.ChooseOnlyButtonValue
            };
        };

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

    public FormStep DeleteAnswerMessage()
    {
        IsDeleteAnswer = true;

        return this;
    }

    public FormStep DisableWebPagePreview()
    {
        IsDisableWebPagePreview = true;

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

    public FormStep ExecuteAnotherForm<TForm>(string? answerCondition = null)
        where TForm : IFormRegistration
    {
        answerCondition ??= string.Empty;
        _nextForms.Add(answerCondition, typeof(TForm));
        return this;
    }

    public async Task<FormStep> BuildAsync(TelegramUserDataEntity user, Dictionary<string, string> data, 
        TelegramFormMessageHandler formMessageHandler,
        CancellationToken cancellationToken = default)
    {
        return _buildFunc(new BuildFormStepEventArgs()
        {
            UserData = user,
            Data = data,
            Builder = new FormStep(),
            FormMessageHandler = formMessageHandler
        });
    }
}