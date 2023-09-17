using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

    public static void DeleteAccount(string accId)
    {
        if(Accounts.RemoveWhere(p => p.AccId == accId) == 1)
        {
            Console.WriteLine("Removed one Account");
            SaveAccounts();
            return;
        }
        Console.WriteLine("No Account found");
    }

    public static async Task FindExport(string? disk = null)
    {
        if (disk is not null)
        {
            if (await SearchFile(disk.ToUpper() + ":\\")) return;
            Console.WriteLine("Found No File");
            return;
        }
        
        var allDrives = DriveInfo.GetDrives();
        foreach (var drive in allDrives)
        {
            if (await SearchFile(drive.Name))
            {
                return;
            }
        }
        Console.WriteLine("Found No File");
    }

    private static async Task<bool> SearchFile(string directory)
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(directory, "*.export");
        }
        catch
        {
            return false;
        }
        foreach (var file in files)
        {
            Console.Write($"Found File {file}. Is this the right one (y/n):\n\r> ");
            var input = Console.ReadLine();
            if (input.ToLower() == "n") continue;

            Console.Write("Do you want to load it (y/n):\n\r> ");

            input = Console.ReadLine();
            if (input.ToLower() == "n") return true;
            await ImportAccount(file);
            return true;
        }

        var directories = Directory.GetDirectories(directory);
        foreach (var d in directories)
        {
            if (await SearchFile(d)) return true;
        }

        return false;
    }

    public static async Task ImportAccount(string path)
    {
        var accJson = await File.ReadAllTextAsync(path);

        var acc = JsonSerializer.Deserialize<Account>(accJson);

        if (Accounts.Any(p => p.AccId == acc.AccId))
        {
            Console.WriteLine("Account Id already existent");
            return;
        }

        Accounts.Add(acc);
        SaveAccounts();
    }

    public static async Task ExportAccount(string accId)
    {
        await ExportAccount(accId, AppDomain.CurrentDomain.BaseDirectory + "\\acc.export");
    }

    public static async Task ExportAccount(string accId, string path)
    {
        var acc = Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }

        var accJson = JsonConvert.SerializeObject(acc);

        try
        {
            await using var wr = new StreamWriter(path);
            await wr.WriteLineAsync(accJson);
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        Console.WriteLine($"Saved to {path}");
    }

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

    public static async Task AddAccount(List<string> args, bool generate)
    {
        if (!args.Contains("--accId"))
        {
            Console.WriteLine("missing accId");
            return;
        }

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
                    acc.AccId = args[i];
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
        
        Accounts.Add(acc);

        if(generate)
        {
            if (acc.PublicKey is null || acc.PrivateKey is null)
            {
                Console.WriteLine("Generating RSA Keys");
                var keyPair = Cryptography.GenerateKey(2048);
                acc.PrivateKey = keyPair.Item2;
                acc.PublicKey = keyPair.Item1;
            }

            if (acc.AesKey is null)
            {
                Console.WriteLine("Generating AES Key");
                acc.AesKey = Cryptography.GenerateAesKey();
            }
            if (acc.UserId is null && acc.CloudIp is not null)
            {
                Console.WriteLine("Generating User");
                await CloudRequest.CreateUserInCloud(acc.AccId, true, false);
            }
        }
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

        for (int i = 1; i < args.Count; i++)
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
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\savedata\\accounts.txt";

        try
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\savedata");
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
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\savedata\\accounts.txt";

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