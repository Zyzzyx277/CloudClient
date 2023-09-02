using System.Text;

namespace CloudClientConsole.tests;

public class CryptographyTest
{
    public static bool TestAes()
    {
        var originalText = Guid.NewGuid().ToString();

        var key = Cryptography.GenerateAesKey();

        var encryptedText = Cryptography.EncryptAes(Encoding.UTF8.GetBytes(originalText), key);

        //var encryptedTextString = Convert.ToBase64String(encryptedText);

        //encryptedText = Convert.FromBase64String(encryptedTextString);

        var decryptedText = Cryptography.DecryptAes(encryptedText, key);

        Console.WriteLine(originalText);
        Console.WriteLine(Encoding.UTF8.GetString(decryptedText));
        
        if (originalText != Encoding.UTF8.GetString(decryptedText)) return false;
        
        return true;
    }
}