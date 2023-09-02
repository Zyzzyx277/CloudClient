namespace CloudClientConsole;

public class FileObjectCloud
{
    public byte[] Content { get; set; }
    public string IdUser { get; set; }
    public string IdFile { get; set; }
    public byte[] Name { get; set; }

    public FileObjectCloud(byte[] content, string idUser, string idFile, byte[] name)
    {
        Content = content;
        IdUser = idUser;
        IdFile = idFile;
        Name = name;
    }
}