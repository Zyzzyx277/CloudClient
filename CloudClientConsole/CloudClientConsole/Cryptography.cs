using System.Security.Cryptography;
using System.Text;

namespace CloudClientConsole;

public class Cryptography
{
    public static string Encrypt(string textToEncrypt, string publicKeyString)
    {
        var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

        using var rsa = new RSACryptoServiceProvider(2048);
        try
        {
            rsa.ImportFromPem(publicKeyString);
            var encryptedData = rsa.Encrypt(bytesToEncrypt, true);
            var base64Encrypted = Convert.ToBase64String(encryptedData);
            return base64Encrypted;
        }
        finally
        {
            rsa.PersistKeyInCsp = false;
        }
    }
    
    public static string Decrypt(string textToDecrypt, string privateKeyString)
    {

        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            try
            {

                // server decrypting data with private key                    
                rsa.ImportFromPem(privateKeyString);

                var resultBytes = Convert.FromBase64String(textToDecrypt);
                var decryptedBytes = rsa.Decrypt(resultBytes, true);
                var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData.ToString();
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    public static byte[] GenerateAesKey(int keySizeByte=32)
    {
        using var aes = Aes.Create();
        // Generate a new random AES key
        aes.KeySize = keySizeByte * 8;
        aes.GenerateKey();

        return aes.Key;
    }
    
    public static byte[] EncryptAes(byte[] plainText, byte[] key)
    {
        using var aes = Aes.Create();
        aes.KeySize = key.Length * 8;
        aes.Key = key;

        // Generate a random IV (Initialization Vector) for added security
        aes.GenerateIV();
        byte[] iv = aes.IV;

        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
        // Write the IV to the stream (it will be needed for decryption)
        msEncrypt.Write(iv, 0, iv.Length);

        // Write the plaintext to the stream
        swEncrypt.Write(plainText);

        return msEncrypt.ToArray();
    }

    public static byte[] DecryptAes(byte[] encryptedBytes, byte[] key)
    {
        using var aes = Aes.Create();
        aes.KeySize = key.Length * 8;
        aes.Key = key;

        // Get the IV from the beginning of the encrypted text
        byte[] iv = new byte[aes.BlockSize / 8];
        Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using MemoryStream msDecrypt = new MemoryStream();
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Write);
        // Return the decrypted text
        return msDecrypt.ToArray();
    }
}