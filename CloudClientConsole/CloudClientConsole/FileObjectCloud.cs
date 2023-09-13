namespace CloudClientConsole;

public class FileObjectCloud
{
    public string Content { get; set; }
    public string IdUser { get; set; }
    public string Path { get; set; }
    public string Id { get; set; }

    public FileObjectCloud(string content, string idUser, string path, string id)
    {
        Content = content;
        IdUser = idUser;
        Path = path;
        Id = id;
    }
}