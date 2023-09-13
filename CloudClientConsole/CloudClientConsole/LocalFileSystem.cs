using CloudClientConsole.TreeStructure;

namespace CloudClientConsole;

public class LocalFileSystem
{
    public static readonly Dictionary<string, Paths> paths = new ();
    public static string? id;

    public static void ChaneId(string accId)
    {
        id = accId;
        Console.WriteLine($"Id changed to {id}");
    }
    
    public static void ChangeCurrentDirectory(string accId, string path)
    {
        if (!paths.ContainsKey(accId))
        {
            Console.WriteLine("Account has no local Files");
            return;
        }

        paths[accId].CurrentPath = paths[accId].PathsTree.GetDirectory(PathToArray(path));
        Console.WriteLine($"CurrentPath: {path}");
    }
    
    public static void ShowCurrentId()
    {
        if (id is null)
        {
            Console.WriteLine("Id not set");
            return;
        }
        Console.WriteLine($"Id: {id}");
    }

    public static void ShowCurrentDirectory(string accId)
    {
        var directory = paths.TryGetValue(accId, out var path) ? path.CurrentPath.name : "Account has no local Files";
        if (directory == "") directory = "/";
        Console.WriteLine(directory);
    }

    public static void ShowFiles(string accId, bool showAll=false)
    {
        if (!paths.ContainsKey(accId))
        {
            Console.WriteLine("Account Not Found");
            return;
        }

        if (showAll)
        {
            foreach (var file in paths[accId].PathsTree.ListAllFiles())
            {
                Console.Write(file.Item1 + "   ");
            }
            Console.WriteLine("");
            return;
        }
        
        foreach (var file in paths[accId].CurrentPath.ListFiles())
        {
            Console.Write(new FileInfo(file.Item1).Name + "   ");
        }
        
        foreach (var directory in paths[accId].CurrentPath.ListDirectories())
        {
            Console.Write(directory + "/   ");
        }
        Console.WriteLine("");
    }

    public class Paths
    {
        public readonly Tree PathsTree = new Tree();
        public Node CurrentPath {get;set;}

        public Paths(IEnumerable<(string, string)> paths)
        {
            foreach (var path in paths)
            {
                PathsTree.AddFile(PathToArray(path.Item1), path);
            }
            CurrentPath = PathsTree.GetDirectory(new List<string>{""});
        }
    }

    public static List<string> PathToArray(string path)
    {
        var pathList = new List<string>();
        pathList.Add("");
        if (path[0] == '/') path.Remove(1);
        
        foreach (var c in path)
        {
            if(c == '/')
            {
                pathList.Add("");
                continue;
            }
            pathList[^1] += c;
        }

        return pathList;
    }
}