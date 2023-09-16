﻿namespace CloudClientConsole.TreeStructure;

public class Node
{
    public string Name { get; private set; }
    public Node? Parent { get; private set; }
    private HashSet<(string, string)> files = new ();
    private HashSet<Node> directories = new();

    public Node(string name, Node? parent)
    {
        Name = name;
        Parent = parent;
    }

    public IEnumerable<(string, string)> ListAllFiles(IEnumerable<(string, string)> list)
    {
        list = list.Concat(files);
        foreach (var directory in directories)
        {
            list = directory.ListAllFiles(list);
        }

        return list;
    }

    public IEnumerable<(string, string)> ListFiles()
    {
        return files;
    }
    
    public IEnumerable<string> ListDirectories()
    {
        return directories.Select(p => p.Name);
    }

    public Node GetDirectory(List<string> path)
    {
        if (Name == path[^1]) return this;
        if (path.Count == 1) throw new Exception("Directory Not Found");
        path.RemoveAt(0);
        var directory = directories.FirstOrDefault(p => p.Name == path[0]);
        if (directory is null) throw new Exception("Directory Not Found");
        
        return directory.GetDirectory(path);
    }

    public void AddDirectory(List<string> path)
    {
        if (path.Count == 1)
        {
            directories.Add(new Node(path[0], this));
            return;
        }

        var directory = directories.FirstOrDefault(p => p.Name == path[0]);

        if (directory is null)
        {
            directory = new Node(path[0], this);
            directories.Add(directory);
        }
        path.RemoveAt(0);
        directory.AddDirectory(path);
    }
    
    public void AddFile(List<string> path, (string, string) file)
    {
        if (path.Count == 1)
        {
            files.Add(file);
            return;
        }

        var directory = directories.FirstOrDefault(p => p.Name == path[0]);

        if (directory is null)
        {
            directory = new Node(path[0], this);
            directories.Add(directory);
        }
        path.RemoveAt(0);
        directory.AddFile(path, file);
    }

    public HashSet<(string, string)> GetFiles(List<string> path)
    {
        if (!path.Any()) return files;

        var directory = directories.FirstOrDefault(p => p.Name == path[0]);
        if (directory is null) throw new Exception("path not valid");
        
        path.RemoveAt(0);
        return directory.GetFiles(path);
    }
    
    public HashSet<string> GetDirectories(List<string> path)
    {
        if (!path.Any()) return directories.Select(p => p.Name).ToHashSet();

        var directory = directories.FirstOrDefault(p => p.Name == path[0]);
        if (directory is null) throw new Exception("path not valid");
        
        path.RemoveAt(0);
        return directory.GetDirectories(path);
    }
    
    public bool ContainsFile(string name)
    {
        return files.Any(p => p.Item1 == name) || 
               directories.Any(directory => directory.ContainsFile(name));
    }

    public string GetPathString(string path = "")
    {
        return Parent is null ? $"{Name}/{path}" : Parent.GetPathString($"{Name}/{path}");
    }
    
    public Stack<string> GetPath(Stack<string> path)
    {
        path.Push(Name);
        if (Parent is not null) path = Parent.GetPath(path);
        return path;
    }
    
    public bool ContainsDirectory(string name)
    {
        return this.Name == name || 
               directories.Any(directory => directory.ContainsDirectory(name));
    }

    public (string, string)? GetFile(List<string> path, string id)
    {
        path.RemoveAt(0);
        if (path.Count > 1)
        {
            foreach (var d in directories.Where(d => d.Name == path[0]))
            {
                return d.GetFile(path, id);
            }

            return null;
        }
        
        var file = files.Where(p => LocalFileSystem.PathToArray(p.Item1, id)[^1] == path[^1]);
        if (!file.Any()) return null;
        
        return file.First();
    }
}