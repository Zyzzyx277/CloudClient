using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CloudClientConsole;

public class CloudRequest
{
    private static string? key;
    public static async Task RequestCommand(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "file":
                await FileCommands(args);
                break;
            case "user":
                await UserCommands(args);
                break;
            case "auth":
                RequestAuth(args);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }

    public static async Task UserCommands(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "create":
                await CreateUserInCloud(args);
                break;
            case "ls":
                await ListUsers(args);
                break;
            case "get":
                await GetUser(args);
                break;
            case "rm":
                DeleteUserFromCloud(args);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
    
    public static async Task FileCommands(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "ls":
                await GetFilesFromCloud(args);
                break;
            case "upload":
                await UploadFile(args);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }

    public static async Task GetUser(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;
        
        using HttpClient client = new HttpClient();

        var response = await client.GetAsync($"http://{acc.CloudIp}/api/Users/{acc.UserId}");
        
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }

        var user = JsonSerializer.Deserialize<User>(content.Replace("id", "Id").Replace("publicKey", "PublicKey"));

        if (user is null)
        {
            Console.WriteLine("No matching User");
            return;
        }
        
        Console.WriteLine($"Id: {user.Id}");
        Console.WriteLine($"Public Key: {user.PublicKey}");
    }

    public static async Task UploadFile(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;

        var content = await File.ReadAllBytesAsync(args[1]);
        var fileInfo = new FileInfo(args[1]);
        var name = Cryptography.EncryptAes(Encoding.UTF8.GetBytes(fileInfo.Name), acc.AesKey);
        
        content = Cryptography.EncryptAes(content, acc.AesKey);
        
        using HttpClient client = new HttpClient();

        FileObjectCloud file = new FileObjectCloud(content, acc.UserId, Guid.NewGuid().ToString(), name);

        var response = await client.PutAsync($"http://{acc.CloudIp}/api/Files/{acc.UserId}/{key}",
            new StringContent(JsonConvert.SerializeObject(file), Encoding.UTF8, "application/json"));
        Console.WriteLine(response.StatusCode);
    }

    public static async Task ListUsers(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;
        
        using HttpClient client = new HttpClient();

        var response = await client.GetAsync($"http://{acc.CloudIp}/api/Users");
        
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }

        var userList = JsonSerializer.Deserialize<IEnumerable<User>>(content.Replace("id", "Id").Replace("publicKey", "PublicKey"));
        foreach (var user in userList)
        {
            Console.WriteLine($"Id: {user.Id}");
            Console.WriteLine($"Public Key: {user.PublicKey}");
            Console.WriteLine("---------------------");
        }
    }

    public static async Task CreateUserInCloud(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;
        
        using HttpClient client = new HttpClient();
        
        var response = await client.PutAsync($"http://{acc.CloudIp}/api/Users", new StringContent(JsonConvert.SerializeObject(acc.PublicKey), Encoding.UTF8, "application/json"));

        string content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }
        Console.WriteLine($"New User: {content}");

        if (args.Contains("-r"))
        {
            acc.UserId = content;
            Account.SaveAccounts();
            Console.WriteLine("UserId updated");
        }
    }

    public static async Task GetFilesFromCloud(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;
        
        using HttpClient client = new HttpClient();

        var response = await client.GetAsync($"http://{acc.CloudIp}/api/Files/{acc.UserId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }

        string responseJson = await response.Content.ReadAsStringAsync();

        var files = JsonSerializer.Deserialize<IEnumerable<string>>(responseJson);
        
        Console.WriteLine(responseJson);

        foreach (var name in files)
        {
            Console.WriteLine(Cryptography.DecryptAes(Convert.FromBase64String(name), acc.AesKey));
            Console.WriteLine("File: " + Encoding.UTF8.GetString(Cryptography.DecryptAes(Convert.FromBase64String(name), acc.AesKey)));
        }
    }

    public static async void RequestAuth(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;
        
        using HttpClient client = new HttpClient();

        var response = await client.PostAsync($"http://{acc.CloudIp}/api/Users/{acc.UserId}", null);
        
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }
        Console.WriteLine($"KeyEncrypted: {content}");

        key = Cryptography.Decrypt(content, acc.PrivateKey);
        
        Console.WriteLine($"KeyDecrypted: {key}");
    }

    public static async void DeleteUserFromCloud(List<string> args)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == args[0]);
        if (acc is null) return;
        
        using HttpClient client = new HttpClient();

        var response = await client.DeleteAsync($"http://{acc.CloudIp}/api/Users/{acc.UserId}/{key}");
        
        string content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }
        Console.WriteLine($"KeyEncrypted: {content}");
    }
}