namespace CloudClientConsole;

public class FileObject
{
    public string Name { get; set; }
    public string Id { get; set; }
    public FileInfo? File { get; set; }

    public static HashSet<FileObject> Files = new();
}