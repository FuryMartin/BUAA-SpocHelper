using System.Security.Cryptography;
using System.Text;

namespace SpocHelper.Core.Helpers;

public static class AESHelper
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("YG5^v%bb&FX6YS!Y");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("36JGQ%bYssG$k*v#");

    public static string Encrypt(string plainText)
    {
        if (plainText == null)
        {
            return null;
        }

        byte[] encrypted;
        var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        encrypted = ms.ToArray();

        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText)
    {
        if (cipherText == null)
        {
            return null;
        }

        var cipherBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
