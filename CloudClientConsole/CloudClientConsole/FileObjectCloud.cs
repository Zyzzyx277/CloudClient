namespace CloudClientConsole;

public class FileObjectCloud
{
    public string Content { get; set; }
    public string IdUser { get; set; }
    public string Path { get; set; }

    public FileObjectCloud(string content, string idUser, string path)
    {
        Content = content;
        IdUser = idUser;
        Path = path;
    }
}