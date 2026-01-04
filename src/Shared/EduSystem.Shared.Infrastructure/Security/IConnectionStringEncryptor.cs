namespace EduSystem.Shared.Infrastructure.Security;

public interface IConnectionStringEncryptor
{
    bool Encrypt(string plainText, out string encrypted);
    bool Decrypt(string cipherText, out string decrypted);
}