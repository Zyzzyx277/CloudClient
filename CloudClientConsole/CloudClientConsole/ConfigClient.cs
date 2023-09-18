namespace CloudClientConsole;

public class ConfigClient
{
    public ConfigClient(string path, string fileId, bool compressed)
    {
        Path = path;
        FileId = fileId;
        Compressed = compressed;
    }

    public string Path { get; set; }
    public string FileId { get; set; }
    public bool Compressed { get; set; }
}