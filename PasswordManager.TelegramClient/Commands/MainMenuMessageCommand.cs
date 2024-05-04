using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.TelegramClient.Commands;

public class MainMenuMessageCommand(IUserDataRepository userDataRepository, ITelegramCommandResolver commandResolver): MessageCommand(userDataRepository, commandResolver)
{
    public override Task<bool> IsMatchAsync(Message message, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    protected override async Task<ExecuteTelegramCommandResult> ExecuteCommandAsync(ExecuteTelegramCommandRequest request, CancellationToken cancellationToken = default)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup;
        
        replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[]
            {
                MessageButtons.ShowMyAccounts
            },
        })
        {
            ResizeKeyboard = true
        };
        await request.Client.SendTextMessageAsync(request.Message.Chat.Id, Resources.MessageBodies.WrongMessageWarningBody, 
            replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);

        return new ExecuteTelegramCommandResult();
    }
}