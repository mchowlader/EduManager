using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace EduSystem.Shared.Infrastructure.Security;

public class ConnectionStringEncryptor : IConnectionStringEncryptor
{
    private readonly byte[]? _key;
    private readonly byte[]? _iv;
    private readonly bool _isConfigured;

    public ConnectionStringEncryptor(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"];
        var ivString = configuration["Encryption:IV"];

        if (!string.IsNullOrEmpty(keyString) && !string.IsNullOrEmpty(ivString))
        {
            try
            {
                _key = Convert.FromBase64String(keyString);
                _iv = Convert.FromBase64String(ivString);
                _isConfigured = true;
            }
            catch (Exception ex)
            {
                _isConfigured = false;
                Console.WriteLine(ex.ToString()); //need to implement logging
            }
        }
        else
        {
            _isConfigured = false;
            Console.WriteLine("Encryption Key/IV missing in appsettings.json.");
        }
    }

    public bool Decrypt(string cipherText, out string decrypted)
    {
        decrypted = string.Empty;

        if (!_isConfigured || _key is null || _iv is null || string.IsNullOrWhiteSpace(cipherText))
            return false;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var buffer = Convert.FromBase64String(cipherText);

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            decrypted = sr.ReadToEnd();

            return true;
        }
        catch (Exception)
        {
            return false;
            //need to implement logging
        }
    }

    public bool Encrypt(string plainText, out string encrypted)
    {

        encrypted = string.Empty;

        if (!_isConfigured || _key is null || _iv is null || string.IsNullOrEmpty(plainText))
            return false;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);

            encrypted = Convert.ToBase64String(ms.ToArray());

            return true;

        }
        catch (Exception)
        {
            return false;
            //need to implement logging
        }
    }
}