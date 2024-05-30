using System.Text;

namespace PasswordManager.TelegramClient.Resources;

public static class MessageBodiesParametrized
{
    public static string AccountsList(List<AccountInfo> accounts)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < accounts.Count; i++)
        {
            stringBuilder.AppendLine($"{i + 1}. *{accounts[i].WebsiteNickname}* _({accounts[i].Url})_ - {accounts[i].User}");
        }
        return string.Format(MessageBodies.AccountsList, stringBuilder);
    } 
    
    public static string AddAccountFinalStep(string websiteNickname, string url, string username, string password)
    {
        return string.Format(MessageBodies.AddAccountFinalStep, url, websiteNickname, username, password);
    }

    public static string GetCredentialsProvideMasterPassword(string url, string websiteNickname, string username)
    {
        return string.Format(MessageBodies.GetCredentialsProvideMasterPassword, url, websiteNickname, username);
    }
    
    public static string DeleteAccountConfirmation(string url, string websiteNickname, string username)
    {
        return string.Format(MessageBodies.DeleteAccountConfirmation, url, websiteNickname, username);
    }
    
    public static string ChooseWhatChangeInAccount(AccountInfo account)
    {
        return string.Format(MessageBodies.ChooseWhatChangeInAccount, account.Url, account.WebsiteNickname, account.User);
    }
}