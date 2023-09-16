using System.Security.Cryptography;
using System.Text;

namespace CloudClientConsole;

public class Cryptography
{
    
    public static (string, string) GenerateKey(int keySize)
    {
        //lets take a new CSP with a new 2048 bit rsa key pair
        var csp = new RSACryptoServiceProvider(keySize);

        //how to get the private key
        var privKey = csp.ExportRSAPrivateKeyPem();

        //and the public key ...
        var pubKey = csp.ExportRSAPublicKeyPem();

        return (pubKey, privKey);
    }
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
        using var rsa = new RSACryptoServiceProvider(2048);
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
        if (plainText == null || key == null)
            throw new ArgumentNullException();

        using Aes aesAlg = Aes.Create();
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.KeySize = key.Length * 8;
        aesAlg.Key = key;
        aesAlg.GenerateIV(); // Generate a random IV

        using MemoryStream msEncrypt = new MemoryStream();
        // Write the IV to the beginning of the encrypted output
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write);
        csEncrypt.Write(plainText, 0, plainText.Length);
        csEncrypt.FlushFinalBlock();
        return msEncrypt.ToArray();
    }

    public static byte[] DecryptAes(byte[] encryptedBytes, byte[] key)
    {
        if (encryptedBytes == null || key == null)
            throw new ArgumentNullException();

        using Aes aesAlg = Aes.Create();
        int ivSize = aesAlg.BlockSize / 8; // IV size in bytes
        byte[] iv = new byte[ivSize];
        byte[] ciphertext = new byte[encryptedBytes.Length - ivSize];

        // Extract the IV from the beginning of the ciphertext
        Array.Copy(encryptedBytes, iv, ivSize);
        Array.Copy(encryptedBytes, ivSize, ciphertext, 0, ciphertext.Length);

        aesAlg.KeySize = key.Length * 8;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.Key = key;
        aesAlg.IV = iv;

        using MemoryStream msDecrypt = new MemoryStream();
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Write);
        csDecrypt.Write(ciphertext, 0, ciphertext.Length);
        csDecrypt.FlushFinalBlock();
        
        return msDecrypt.ToArray();
    }
    
    public static async Task EncryptStreamAes(Stream inputStream, byte[] key, Stream outputStream)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.KeySize = key.Length * 8;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.Key = key;
        aesAlg.GenerateIV();

        // Create an encryptor to perform the stream transform
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        // Create a CryptoStream to write the encrypted data to the output stream
        await using CryptoStream csEncrypt = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
            
        await inputStream.CopyToAsync(csEncrypt);
        await csEncrypt.FlushFinalBlockAsync();
    }
    public static async Task DecryptStreamAes(Stream inputStream, byte[] key, byte[] iv, Stream outputStream)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.KeySize = key.Length * 8;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.Key = key;
        aesAlg.IV = iv;

        // Create a decryptor to perform the stream transform
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // Create a CryptoStream to read the encrypted data from the input stream
        await using (CryptoStream csDecrypt = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
        {
            await csDecrypt.CopyToAsync(outputStream);
        }

        // Reset the position of the output stream to the beginning
        outputStream.Position = 0;
    }
    
}