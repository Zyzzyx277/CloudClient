using System.Net;
using System.Text;
using System.Text.Json;

namespace CloudClientConsole;

public class Account
{
    public string? PrivateKey { get; set; }
    public string? PublicKey { get; set; }
    public byte[]? AesKey { get; set; }
    public string? CloudIp { get; set; }
    public string? UserId { get; set; }
    public string AccId { get; set; }

    public static readonly HashSet<Account> Accounts = LoadAccounts();

    public static void ShowAccounts()
    {
        Console.WriteLine("Accounts:\n\r-----------------------");
        foreach (var acc in Accounts)
        {
            Console.WriteLine($"AccId: {acc.AccId}");
            Console.WriteLine($"UserId: {acc.UserId}");
            Console.WriteLine($"CloudIp: {acc.CloudIp}");
            Console.WriteLine("-----------------------");
        }
    }

    public static async Task AddAccount(List<string> args)
    {
        Account acc = new Account();
        
        for (int i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--privateKey":
                    i++;
                    acc.PrivateKey = args[i];
                    break;
                case "--publicKey":
                    i++;
                    acc.PublicKey = args[i];
                    break;
                case "--userId":
                    i++;
                    acc.UserId = args[i];
                    break;
                case "--aesKey":
                    i++;
                    acc.AesKey = Encoding.UTF8.GetBytes(args[i]);
                    break;
                case "--cloudIp":
                    i++;
                    acc.CloudIp = args[i];
                    break;
                case "--accId":
                    i++;
                    acc.AccId =args[i];
                    break;
                case "--aesKeyPath":
                    i++;
                    acc.AesKey = await File.ReadAllBytesAsync(args[i]);
                    break;
                case "--publicKeyPath":
                    i++;
                    acc.PublicKey = await File.ReadAllTextAsync(args[i]);
                    break;
                case "--privateKeyPath":
                    i++;
                    acc.PrivateKey = await File.ReadAllTextAsync(args[i]);
                    break;
            }
        }

        if (Accounts.Any(p => (p.UserId == acc.UserId && p.CloudIp.Equals(acc.CloudIp)) || p.AccId == acc.AccId))
        {
            Console.WriteLine("Account already existent");
            return;
        }

        if (acc.UserId is null && acc.CloudIp is not null) await CloudRequest.CreateUserInCloud(acc.AccId, true);
        
        Accounts.Add(acc);
        Console.WriteLine("Account created");
        SaveAccounts();
    }

    public static void GetProp(string accId, string prop)
    {
        var acc = Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }

        string output = string.Empty;
        switch (prop)
        {
            case "privateKey":
                output = acc.PrivateKey;
                break;
            case "publicKey":
                output = acc.PublicKey;
                break;
            case "aesKey":
                output = Encoding.UTF8.GetString(acc.AesKey);
                break;
            case "userId":
                output = acc.UserId;
                break;
            case "cloudIp":
                output = acc.CloudIp;
                break;
            default:
                Console.WriteLine($"{prop}: Argument Not Found");
                return;
        }

        Console.WriteLine($"{prop}: {output}");
    }

    public static void UpdateAccount(List<string> args)
    {
        var acc = Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;

        for(int i = 1; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--privateKey":
                    i++;
                    acc.PrivateKey = args[i];
                    break;
                case "--publicKey":
                    i++;
                    acc.PublicKey = args[i];
                    break;
                case "--aesKey":
                    i++;
                    acc.AesKey = Convert.FromBase64String(args[i]);
                    break;
                case "--userId":
                    if (Accounts.Any(p => p.UserId == args[2] && p.CloudIp == acc.CloudIp))
                    {
                        Console.WriteLine("Account already existent");
                        return;
                    }
                    i++;
                    acc.UserId = args[i];
                    break;
                case "--accId":
                    if (Accounts.Any(p => p.AccId == args[2]))
                    {
                        Console.WriteLine("Account already existent");
                        return;
                    }
                    i++;
                    acc.AccId = args[i];
                    break;
                case "--cloudIp":
                    if (Accounts.Any(p => p.UserId == acc.UserId && p.CloudIp == args[2]))
                    {
                        Console.WriteLine("Account already existent");
                        return;
                    }
                    i++;
                    acc.CloudIp = args[i];
                    break;
                case "--aesKeyPath":
                    i++;
                    acc.AesKey = File.ReadAllBytes(args[i]);
                    break;
                case "--privateKeyPath":
                    i++;
                    acc.PrivateKey = File.ReadAllText(args[i]);
                    break;
                case "--publicKeyPath":
                    i++;
                    acc.PublicKey = File.ReadAllText(args[i]);
                    break;
                default:
                    Console.WriteLine($"{args[i]}: Argument Not Found");
                    return;
            }
        }
        SaveAccounts();
    }

    public static void SaveAccounts()
    {
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\savedata\\accounts.txt";
        
        try
        {
            string json = JsonSerializer.Serialize(Accounts);
            File.WriteAllText(filePath, json);
            
            Console.WriteLine("HashSet saved to file successfully.");
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error saving HashSet to file: " + ex.Message);
        }
    }

    public static HashSet<Account> LoadAccounts()
    {
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\savedata\\accounts.txt";

        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                HashSet<Account>? loadedHashSet = JsonSerializer.Deserialize<HashSet<Account>>(json);

                if (loadedHashSet != null)
                {
                    return loadedHashSet;
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error loading HashSet from file: " + ex.Message);
        }

        return new HashSet<Account>();
    }
}