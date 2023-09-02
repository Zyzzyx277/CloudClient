namespace CloudClientConsole;

public class LocalFileSystem
{
    public static readonly Dictionary<string, Paths> paths = new ();

    public static void ShowFiles(string accId, bool showAll=false)
    {
        if (!paths.ContainsKey(accId))
        {
            Console.WriteLine("Account Not Found");
            return;
        }

        var pathsObject = paths[accId];
        var files = pathsObject.paths;

        if (!showAll) files = pathsObject.paths.Where(p =>
                p.Remove(pathsObject.currentPath.Length, p.Length - 1) == pathsObject.currentPath);

        foreach (var file in files)
        {
            if (showAll) Console.Write(file + "   ");
            else Console.Write(new FileInfo(file).Name + "  ");
        }   
        Console.WriteLine("");
    }

    public class Paths
    {
        public IEnumerable<string> paths;
        public string currentPath;

        public Paths(IEnumerable<string> paths, string currentPath)
        {
            this.paths = paths;
            this.currentPath = currentPath;
        }
    }
}