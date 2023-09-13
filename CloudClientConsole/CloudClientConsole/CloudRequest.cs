using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CloudClientConsole;

public class CloudRequest
{
    private static string? key;

    public static async Task DeleteFileFromCloud(string accId, string fileId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return;
        }

        if (key is null)
        {
            Console.WriteLine("No authentication key");
            return;
        }

        if (!await GetFilesFromCloud(accId)) return;

        if (!LocalFileSystem.paths.ContainsKey(accId))
        {
            Console.WriteLine("Account has no local Files");
            return;
        }

        var filePaths = LocalFileSystem.paths[accId].PathsTree.GetFile(LocalFileSystem.PathToArray(fileId));

        if (filePaths is null)
        {
            Console.WriteLine("File Not Found");
            return;
        }

        var fileIdEncrypted = (((string, string))filePaths).Item2;

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"http://{acc.CloudIp}/api/Files/{acc.UserId}/{key}/{fileIdEncrypted}");
        request.Content =
            new StringContent(JsonConvert.SerializeObject(fileIdEncrypted), Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);

        Console.WriteLine(response.StatusCode);

        await GetFilesFromCloud(accId);
    }

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

        var user = JsonSerializer.Deserialize<User?>(content);

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
        if (!await GetFilesFromCloud(accId)) return;

        var acc = Account.Accounts.First(p => p.AccId == accId);
        
        var files = LocalFileSystem.paths[accId].PathsTree.ListAllFiles().ToList();
        if (files.Any(p => p.Item1 == filePath))
        {
            Console.WriteLine("File already existent");
            return;
        }

        string uploadBuffer = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\uploadBuffer";

        if(!await ChunkFile(filePath, uploadBuffer, acc.AesKey)) return;
        
        var existingIds = files.Select(p => p.Item2).ToList();
        string newId;
        do
        {
            newId = Guid.NewGuid().ToString();
        } while (existingIds.Contains(newId));

        pathCloud = Convert.ToBase64String(Cryptography.EncryptAes(Encoding.UTF8.GetBytes(pathCloud), acc.AesKey));
        
        string uri = $"http://{acc.CloudIp}/api/Files/{acc.UserId}/{key}/{newId}/{pathCloud}";

        await using (var fs = new FileStream(uploadBuffer + "/encrypted.dat", FileMode.Open))
        {
            var fileContent = new StreamContent(fs);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            fileContent.Headers.ContentLength = null;
            var formData = new MultipartFormDataContent();
            formData.Add(fileContent, "file", "file-name.txt");

            try
            {
                using HttpClient client = new HttpClient();
                Console.WriteLine("Uploading File");
                var response = await client.PutAsync(uri, formData);

                if (!response.IsSuccessStatusCode) Console.WriteLine($"{response.StatusCode}({response.ReasonPhrase})");
            }
            catch
            {
                Console.WriteLine("Connection not possible");
            }
        }
        
        File.Delete(uploadBuffer + "/encrypted.dat");
        
        await GetFilesFromCloud(accId);
    }
    
    private static async Task<bool> ChunkFile(string sourceFilePath, string outputFolderPath, byte[] key)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine("Source file does not exist.");
            return false;
        }

        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        Console.WriteLine("Encrypting File");
        await using(FileStream fsOpen = new FileStream(sourceFilePath, FileMode.Open))
        {
            await using (FileStream fsCreate = new FileStream(outputFolderPath + "/encrypted.dat", FileMode.Create))
            {
                await Cryptography.EncryptStreamAes(fsOpen, key, fsCreate);
            }
        }
        
        Console.WriteLine("Encryption Complete");
        return true;
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

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }

        var userList = JsonSerializer.Deserialize<IEnumerable<User>>(content);
        Console.WriteLine("---------------------");
        if(userList is null) return;
        foreach (var user in userList)
        {
            var accUser = Account.Accounts.FirstOrDefault(p => p.UserId == user.Id && p.CloudIp == acc.CloudIp);
            Console.WriteLine(accUser is null ? "AccId: No Account" : $"AccId: {accUser.AccId}");
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

        var response = await client.PutAsync($"http://{acc.CloudIp}/api/Users",
            new StringContent(JsonConvert.SerializeObject(acc.PublicKey), Encoding.UTF8, "application/json"));

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

    public static async Task PullFile(string accId, string fileId, string path)
    {
        if (!await GetFilesFromCloud(accId)) return;
        
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);

        using HttpClient client = new HttpClient();

        if (!LocalFileSystem.paths.ContainsKey(accId))
        {
            Console.WriteLine("Account has no local files");
            return;
        }

        var filePaths = LocalFileSystem.paths[accId].PathsTree.GetFile(LocalFileSystem.PathToArray(fileId));

        if (filePaths is null)
        {
            Console.WriteLine("File Not Found");
            return;
        }

        string fileCloudId = (((string, string))filePaths).Item2;

        var response = await client.GetAsync($"http://{acc.CloudIp}/api/Files/{acc.UserId}/{fileCloudId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"{response.StatusCode}({response.ReasonPhrase})");
            return;
        }
        
        string downloadBuffer = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\downloadBuffer";
        byte[] iv = new byte[16];
        Stream stream = await response.Content.ReadAsStreamAsync();
        await stream.ReadAsync(iv, 0, 16);
        stream.Position = 16;
        await using (FileStream fsEncrypted = new FileStream(downloadBuffer + "\\encrypted.dat", FileMode.Create))
        {
            await stream.CopyToAsync(fsEncrypted);
        }

        await using (FileStream fsOpen = new FileStream(downloadBuffer + "\\encrypted.dat", FileMode.Open))
        {
            await using (FileStream fsCreate = new FileStream(path, FileMode.Create))
            {
                Console.WriteLine("Decrypting File");
                await Cryptography.DecryptStreamAes(fsOpen, acc.AesKey, iv, fsCreate);
            }
        }
        
        File.Delete(downloadBuffer + "\\encrypted.dat");
        
        Console.WriteLine($"File {path} created");
    }

    public static async Task<bool> GetFilesFromCloud(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null)
        {
            Console.WriteLine("Account Not Found");
            return false;
        }

        using HttpClient client = new HttpClient();

        var response = await client.GetAsync($"http://{acc.CloudIp}/api/Files/{acc.UserId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return false;
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        List<TupleCloud>? files;

        try
        {
            files = JsonSerializer.Deserialize<List<TupleCloud>>(responseJson);
        }
        catch (JsonException)
        {
            Console.WriteLine("Cloud answer could not be read");
            return false;
        }

        files ??= new List<TupleCloud>();
        var listTuple = new List<(string, string)>();

        foreach (var valueTuple in files)
        {
            valueTuple.Item1 =
                Encoding.UTF8.GetString(Cryptography.DecryptAes(Convert.FromBase64String(valueTuple.Item1), acc.AesKey));
            listTuple.Add((valueTuple.Item1, valueTuple.Item2));
        }

        LocalFileSystem.paths[accId] = new LocalFileSystem.Paths(listTuple);

        Console.WriteLine($"Received {files.Count} Files");
        return true;
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

    public static async Task DeleteUserFromCloud(string accId)
    {
        var acc = Account.Accounts.FirstOrDefault(p => p.AccId == accId);
        if (acc is null) return;

        using HttpClient client = new HttpClient();

        var response = await client.DeleteAsync($"http://{acc.CloudIp}/api/Users/{acc.UserId}/{key}");

        Console.WriteLine($"{response.StatusCode}({response.ReasonPhrase})");
    }
}