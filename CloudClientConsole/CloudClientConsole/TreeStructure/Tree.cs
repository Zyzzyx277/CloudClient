using System.Reflection.Metadata.Ecma335;

namespace CloudClientConsole.TreeStructure;

public class Tree
{
    private Node root = new Node("", null);

    public ConfigClient? GetFile(List<string> path, string id)
    {
        return root.GetFile(path, id);
    }

    public bool ContainsPath(List<string> path)
    {
        if (!path.Any()) return false;
        return path is [""] || root.ContainsPath(path.ToList());
    }
    
    public IEnumerable<ConfigClient> ListAllFiles()
    {
        return root.ListAllFiles(Enumerable.Empty<ConfigClient>());
    }

    public Node GetDirectory(List<string> path)
    {
        return root.GetDirectory(path.ToList());
    }

    public void AddDirectory(List<string> path)
    {
        path.RemoveAt(0);
        root.AddDirectory(path);
    }
    
    public void AddFile(List<string> path, ConfigClient file)
    {
        path.RemoveAt(0);
        root.AddFile(path, file);
    }

    public HashSet<ConfigClient> GetFiles(List<string> path)
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
    
    public bool ContainsFile(List<string> path)
    {
        return path.Count > 1 && root.ContainsFile(path.ToList());
    }
    
    public bool ContainsDirectory(string name)
    {
        return root.ContainsDirectory(name);
    }
    
    public bool ContainsDirectory(List<string> path)
    {
        if (!path.Any()) return false;
        return path is [""] || root.ContainsDirectory(path.ToList());
    }
}