using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CloudClientConsole;

public class CloudRequest
{
    private static string? key;

    public static async Task GetUser(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }
        
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

    public static async Task UploadFile(string accId, string filePath, string pathCloud)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc?.UserId is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }

        var content = await File.ReadAllBytesAsync(filePath);
        
        content = Cryptography.EncryptAes(content, acc.AesKey);

        var f = new FileInfo(filePath);

        pathCloud = Convert.ToBase64String(Cryptography.EncryptAes(Encoding.UTF8.GetBytes(pathCloud + f.Name), acc.AesKey));

        using HttpClient client = new HttpClient();

        FileObjectCloud file = new FileObjectCloud(Convert.ToBase64String(content), acc.UserId, pathCloud);

        
        Console.WriteLine(JsonConvert.SerializeObject(file));
        var response = await client.PutAsync($"http://{acc.CloudIp}/api/Files/{acc.UserId}/{key}",
            new StringContent(JsonConvert.SerializeObject(file), Encoding.UTF8, "application/json"));
        Console.WriteLine(response.StatusCode);
    }

    public static async Task ListUsers(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }
        
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

    public static async Task CreateUserInCloud(string accId, bool saveId = false)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }
        
        using HttpClient client = new HttpClient();
        
        var response = await client.PutAsync($"http://{acc.CloudIp}/api/Users", new StringContent(JsonConvert.SerializeObject(acc.PublicKey), Encoding.UTF8, "application/json"));

        string content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }
        Console.WriteLine($"New User: {content}");

        if (saveId)
        {
            acc.UserId = content;
            Account.SaveAccounts();
            Console.WriteLine("UserId updated");
        }
    }

    public static async Task GetFilesFromCloud(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }
        
        using HttpClient client = new HttpClient();

        var response = await client.GetAsync($"http://{acc.CloudIp}/api/Files/{acc.UserId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }

        string responseJson = await response.Content.ReadAsStringAsync();

        var files = JsonSerializer.Deserialize<IEnumerable<string>>(responseJson).ToArray();

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Encoding.UTF8.GetString(Cryptography.DecryptAes(Convert.FromBase64String(files[i]), acc.AesKey));
        }

        LocalFileSystem.paths[accId] = new LocalFileSystem.Paths(files, "/");
    }

    public static async Task RequestAuth(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }

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

    public static async void DeleteUserFromCloud(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
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