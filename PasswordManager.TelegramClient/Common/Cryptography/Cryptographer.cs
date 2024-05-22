using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.TelegramClient.Common.Cryptography;

public class Cryptographer
{
    private const int Rfc2898KeygenIterations= 100;
    private const int AesKeySizeInBits = 128;
    
    public static EncryptResult Encrypt(string plainText, string password)
    {
        var salt = new byte[16];
        var rnd = new Random();
        rnd.NextBytes(salt);
        byte[] rawPlaintext = Encoding.Unicode.GetBytes(plainText);
        byte[] cipherText;

        using (var aes = Aes.Create())
        {
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = AesKeySizeInBits;
            var keyStrengthInBytes = aes.KeySize / 8;

            var rfc2898 = new Rfc2898DeriveBytes(password, salt, Rfc2898KeygenIterations, HashAlgorithmName.SHA256);

            aes.Key = rfc2898.GetBytes(keyStrengthInBytes);
            aes.IV = rfc2898.GetBytes(keyStrengthInBytes);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(rawPlaintext, 0, rawPlaintext.Length);
                }
                cipherText = ms.ToArray();
            }
        }

        return new EncryptResult()
        {
            CipherText = cipherText,
            IV = salt
        };
    }
    
    public static string Decrypt(byte[] cipherText, byte[] iv, string password)
    {
        using var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = AesKeySizeInBits;
        var keyStrengthInBytes = aes.KeySize / 8;

        var rfc2898 = new Rfc2898DeriveBytes(password, iv, Rfc2898KeygenIterations, HashAlgorithmName.SHA256);

        aes.Key = rfc2898.GetBytes(keyStrengthInBytes);
        aes.IV = rfc2898.GetBytes(keyStrengthInBytes);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(cipherText, 0, cipherText.Length);
        }
        
        return Encoding.Unicode.GetString(ms.ToArray());
    }

    public static string GetHash(string input)
    {
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sBuilder = new StringBuilder();

        foreach (var t in data)
        {
            sBuilder.Append(t.ToString("x2"));
        }
        return sBuilder.ToString();
    }
}