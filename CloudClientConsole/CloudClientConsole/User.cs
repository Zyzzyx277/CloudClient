namespace CloudClientConsole;

public class User
{
    public User(string id, string publicKey)
    {
        Id = id;
        PublicKey = publicKey;
    }

    public string Id { get; set; }
    public string PublicKey { get; set; }

    public static HashSet<User> UserDb = new();
}