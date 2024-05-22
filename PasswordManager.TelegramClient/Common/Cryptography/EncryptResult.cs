namespace PasswordManager.TelegramClient.Common.Cryptography;

public class EncryptResult
{
    public byte[] CipherText { get; set; }
    
    public byte[] IV { get; set; }
}