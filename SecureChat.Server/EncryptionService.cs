using System.Security.Cryptography;
using System.Text;

public static class EncryptionService
{
    private static readonly byte[] Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("ENCRYPTION_KEY")!);

    public static string AES_Encrypt(string plaintext)
    {
        using var aes = new AesGcm(Key, AesGcm.TagByteSizes.MaxSize);

        var iv = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        aes.Encrypt(iv, plaintextBytes, ciphertext, tag);

        return Convert.ToBase64String([.. iv, .. tag, .. ciphertext]);
    }

    public static string AES_Decrypt(string encrypted)
    {
        using var aes = new AesGcm(Key, AesGcm.TagByteSizes.MaxSize);

        var data = Convert.FromBase64String(encrypted);
        var iv = data[..12];
        var tag = data[12..28];
        var ciphertext = data[28..];
        var plaintext = new byte[ciphertext.Length];

        aes.Decrypt(iv, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public static string SHA256_Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hashedBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashedBytes);
    }

    public static string HMAC_SHA256_Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hashedBytes = HMACSHA256.HashData(Key, bytes);
        return Convert.ToHexString(hashedBytes);
    }
}
