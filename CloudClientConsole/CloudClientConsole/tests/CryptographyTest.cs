namespace CloudClientConsole.tests;

public class CryptographyTest
{
    public void TestAes()
    {
        var originalText = Guid.NewGuid().ToByteArray();

        var key = Cryptography.GenerateAesKey();

        var encryptedText = Cryptography.EncryptAes(originalText, key);

        var decryptedText = Cryptography.DecryptAes(encryptedText, key);

        if (originalText.Length != decryptedText.Length) return;
        for(int i = 0; i < decryptedText.Length; i++)
        {
            if (originalText[i] != decryptedText[i]) return;
        }
    }
}