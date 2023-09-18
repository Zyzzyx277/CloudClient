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

        if (!IsValidPath(path))
        {
            Console.WriteLine("Path not valid");
            return;
        }

        var pathsObject = paths[accId];
        var pathList = PathToArray(path, accId);
        if (!pathsObject.PathsTree.ContainsDirectory(pathList))
        {
            Console.WriteLine("Path not in local Filesystem");
            return;
        }
            
        pathsObject.CurrentPath = pathsObject.PathsTree.GetDirectory(pathList);
        Console.WriteLine($"CurrentPath: {paths[accId].CurrentPath.GetPathString()}");
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
        var directory = paths.TryGetValue(accId, out var path) ? path.CurrentPath.GetPathString() : "Account has no local Files";
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
                Console.Write(file.Path + "   ");
            }
            Console.WriteLine("");
            return;
        }
        
        foreach (var file in paths[accId].CurrentPath.ListFiles())
        {
            Console.Write(new FileInfo(file.Path).Name + "   ");
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

        public Paths(IEnumerable<ConfigClient> paths, string id)
        {
            foreach (var path in paths)
            {
                PathsTree.AddFile(PathToArray(path.Path, id), path);
            }
            CurrentPath = PathsTree.GetDirectory(new List<string>{""});
        }
    }

    public static List<string> PathToArray(string path, string? id)
    {
        var pathList = new List<string> { "" };

        if (path == "/") return pathList;
        
        foreach (var c in path)
        {
            if(c == '/')
            {
                pathList.Add("");
                continue;
            }
            pathList[^1] += c;
        }

        if (id is not null) pathList = ManageDots(pathList, id);
        return pathList;
    }

    private static List<string> ManageDots(List<string> path, string id)
    {
        if (path.Count < 1) return path;
        
        if (path[0] == ".")
        {
            if (!paths.ContainsKey(id))
            {
                Console.WriteLine("Account has no local Files");
                return path;
            }
            var curPath = paths[id];
            path.RemoveAt(0);
            path = curPath.CurrentPath.GetPath(new Stack<string>()).Concat(path).ToList();
        }
        
        for (int i = 1; i < path.Count; i++)
        {
            if (path[i] != "..") continue;
            if (path.Count < 3) return new List<string> { "" };
            path.RemoveAt(i);
            path.RemoveAt(i - 1);
            i -= 2;
        }

        
        return path;
    }

    public static bool IsValidPath(string path)
    {
        if(path == "/") return true;
        if (path.Length < 2) return false;
        if (path[0] != '/' && path[0] != '.') return false;
        for (int i = 1; i < path.Length; i++)
        {
            if (path[i] == '/' && path[i - 1] == '/') return false;
        }

        return path[^1] != '/';
    }
}