namespace CloudClientConsole.TreeStructure;

public class Tree
{
    private Node root = new Node("");

    public (string, string)? GetFile(List<string> path)
    {
        return root.GetFile(path);
    }
    
    public IEnumerable<(string, string)> ListAllFiles()
    {
        return root.ListAllFiles(Enumerable.Empty<(string, string)>());
    }

    public Node GetDirectory(List<string> path)
    {
        return root.GetDirectory(path);
    }

    public void AddDirectory(List<string> path)
    {
        path.RemoveAt(0);
        root.AddDirectory(path);
    }
    
    public void AddFile(List<string> path, (string, string) file)
    {
        path.RemoveAt(0);
        root.AddFile(path, file);
    }

    public HashSet<(string, string)> GetFiles(List<string> path)
    {
        path.RemoveAt(0);
        return root.GetFiles(path);
    }
    
    public HashSet<string> GetDirectories(List<string> path)
    {
        path.RemoveAt(0);
        return root.GetDirectories(path);
    }
    
    public bool ContainsFile(string name)
    {
        return root.ContainsFile(name);
    }
    
    public bool ContainsDirectory(string name)
    {
        return root.ContainsDirectory(name);
    }
}