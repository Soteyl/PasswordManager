using System.Text;

namespace PasswordManager.TelegramClient.Resources;

public static class MessageBodiesParametrized
{
    public static string AccountsList(List<AccountInfo> accounts)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < accounts.Count; i++)
        {
            stringBuilder.AppendLine($"{i + 1}. {accounts[i].WebsiteNickname} *({accounts[i].Url})* - {accounts[i].User}");
        }
        return string.Format(MessageBodies.AccountsList, stringBuilder);
    } 
}