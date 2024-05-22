using PasswordManager.TelegramClient.Resources;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Common.Keyboard;

public class KeyboardBuilder
{
    private List<List<string>> _buttons = new();
    
    public KeyboardBuilder AddRow(params string[] buttons)
    {
        _buttons.Add(buttons.ToList());
        return this;
    }

    public string[][] Build()
    {
        return _buttons.Select(x => x.ToArray()).ToArray();
    }
    
    public KeyboardBuilder Cancel()
    {
        AddRow(MessageButtons.Cancel);
        return this;
    }
    
    public KeyboardBuilder Return()
    {
        AddRow(MessageButtons.Return);
        return this;
    }
    
    public static IReplyMarkup GetMarkup(IEnumerable<IEnumerable<string>>? buttons)
    {
        buttons = buttons?.ToList();
        return buttons is not null && buttons.Any() ? new ReplyKeyboardMarkup(buttons.Select(row => row.Select(x => new KeyboardButton(x))))
        {
            ResizeKeyboard = true
        } : new ReplyKeyboardRemove();
    }
}