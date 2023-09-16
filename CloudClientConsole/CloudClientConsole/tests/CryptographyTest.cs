using System.Text;

namespace CloudClientConsole.tests;

public class CryptographyTest
{
    public static bool TestAes()
    {
        var originalText = Guid.NewGuid().ToString();

        var key = Cryptography.GenerateAesKey();

        var encryptedText = Cryptography.EncryptAes(Encoding.UTF8.GetBytes(originalText), key);

        Console.WriteLine(encryptedText.Length);
        //var encryptedTextString = Convert.ToBase64String(encryptedText);

        //encryptedText = Convert.FromBase64String(encryptedTextString);

        var decryptedText = Cryptography.DecryptAes(encryptedText, key);

        Console.WriteLine(originalText);
        Console.WriteLine(Encoding.UTF8.GetString(decryptedText));
        
        if (originalText != Encoding.UTF8.GetString(decryptedText)) return false;
        
        return true;
    }
    
    /*public static async Task<bool> TestAesStream()
    {
        await using FileStream ms = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\savedata\\accounts.txt", FileMode.Open);
        var key = Cryptography.GenerateAesKey();

        var encryptedStream = await Cryptography.EncryptStreamAes(ms, key, new MemoryStream());

        var textEncrypted = new byte[encryptedStream.Length];
        await encryptedStream.ReadAsync(textEncrypted, 0, textEncrypted.Length);

        await using var sw = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\savedata\\accounts2.txt", FileMode.Create);
        await Cryptography.DecryptStreamAes(new MemoryStream(textEncrypted), key, sw);

        return true;
    }*/
}