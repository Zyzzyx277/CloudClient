namespace CloudClientConsole;

public class CommandManager
{
    public static async Task ProcessCommand(List<string> args)
    {
        string command = args[0];
        args.RemoveAt(0);
        switch (command.ToLower())
        {
            case "acc":
                await AccCommands(args);
                break;
            case "req":
                await RequestCommand(args);
                break;
            case "loc":
                LocalCommands(args);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }

    public static void LocalCommands(List<string> args)
    {
        string command = args[0];
        args.RemoveAt(0);
        switch (command.ToLower())
        {
            case "ls":
                if (args.All(p => p[0] == '-'))
                {
                    if (LocalFileSystem.id is not null)
                    {
                        if(args.Contains("-a"))LocalFileSystem.ShowFiles(LocalFileSystem.id, true);
                        else LocalFileSystem.ShowFiles(LocalFileSystem.id);
                        return;
                    }
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                if(args.Contains("-a"))LocalFileSystem.ShowFiles(args[0], true);
                else LocalFileSystem.ShowFiles(args[0]);
                break;
            case "cur":
                if (args.Count < 1)
                {
                    if (LocalFileSystem.id is not null)
                    {
                        LocalFileSystem.ShowCurrentDirectory(LocalFileSystem.id);
                        break;
                    }
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                LocalFileSystem.ShowCurrentDirectory(args[0]);
                break;
            case "cd":
                if (args.Count < 2)
                {
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                LocalFileSystem.ChangeCurrentDirectory(args[0], args[1]);
                break;
            case "id":
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                LocalFileSystem.ChaneId(args[0]);
                break;
            case "curId":
                LocalFileSystem.ShowCurrentId();
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
    
    public static async Task RequestCommand(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "file":
                await FileCommands(args);
                break;
            case "user":
                await UserCommands(args);
                break;
            case "auth":
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                await CloudRequest.RequestAuth(args[0]);
                break;
            case "rm":
                if (args.Count < 2)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                Console.Write("This will delete all Data on chosen Server. Proceed(y/n)?\n\r> ");
                string? input = Console.ReadLine();
                if (input is null ||input.ToLower() != "y") return;
                await CloudRequest.DeleteAll(args[0], args[1]);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
    
    public static async Task UserCommands(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "create":
                if (args.All(p => p[0] == '-'))
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                await CloudRequest.CreateUserInCloud(args.Where(p => !p.Contains('-')).ToArray()[0], args.Contains("-r"));
                break;
            case "ls":
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                await CloudRequest.ListUsers(args[0]);
                break;
            case "get":
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                await CloudRequest.GetUser(args[0]);
                break;
            case "rm":
                if (!args.Any(p => p[0] != '-'))
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                if (args.Contains("-t"))
                {
                    args.Remove("-t");
                    await CloudRequest.RequestAuth(args[0]);
                }
                await CloudRequest.DeleteUserFromCloud(args[0]);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
    public static async Task FileCommands(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "ls":
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                }
                await CloudRequest.GetFilesFromCloud(args[0]);
                break;
            case "upload":
                if (args.Count(p => p[0] != '-') < 3)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                bool compress = args.Contains("-c");
                args.Remove("-c");
                if (args.Contains("-t"))
                {
                    args.Remove("-t");
                    await CloudRequest.RequestAuth(args[0]);
                }
                await CloudRequest.UploadFile(args[0], args[1], args[2], compress);
                break;
            case "get":
                if (args.Count < 3)
                {
                    if (LocalFileSystem.id is not null && args.Count >= 2)
                    {
                        await CloudRequest.PullFile(LocalFileSystem.id, args[0], args[1]);
                        break;
                    }
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                await CloudRequest.PullFile(args[0], args[1], args[2]);
                break;
            case "rm":
                if (args.Count(p => !p.Contains('-')) < 2)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                if (args.Contains("-t"))
                {
                    args.Remove("-t");
                    await CloudRequest.RequestAuth(args[0]);
                }
                await CloudRequest.DeleteFileFromCloud(args[0], args[1]);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
    
    public static async Task AccCommands(List<string> args)
    {
        string command = args[0];
        if(args.Count > 0) args.RemoveAt(0);
        
        switch (command.ToLower())
        {
            case "ls":
                Account.ShowAccounts();
                break;
            case "add":
                await Account.AddAccount(args, args.Contains("--generate"));
                break;
            case "update":
                int optionals = args.Count(p => p.Contains('-')) * 2;
                if (args.Count - optionals < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                Account.UpdateAccount(args);
                break;
            case "get":
                if (args.Count < 2)
                {
                    Console.WriteLine("Not enough Arguments");
                }
                Account.GetProp(args[0], args[1]);
                break;
            case "save":
                switch (args.Count)
                {
                    case < 1:
                        Console.WriteLine("Not enough Arguments");
                        return;
                    case 1:
                        await Account.ExportAccount(args[0]);
                        break;
                    default:
                        await Account.ExportAccount(args[0], args[1]);
                        break;
                }
                break;
            case "load":
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                await Account.ImportAccount(args[0]);
                break;
            case "scan":
                if(args.Any()) await Account.FindExport(args[0]);
                else await Account.FindExport();
                break;
            case "rm":
                if (!args.Any())
                {
                    Console.WriteLine("Not enough Arguments");
                    break;
                }
                Account.DeleteAccount(args[0]);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
}