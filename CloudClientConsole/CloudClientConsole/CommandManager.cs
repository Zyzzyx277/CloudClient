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
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                }
                if(args.Count < 2)LocalFileSystem.ShowFiles(args[0]);
                else if(args.Contains("-a"))LocalFileSystem.ShowFiles(args[0], true);
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
                }
                await CloudRequest.RequestAuth(args[0]);
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
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                await CloudRequest.CreateUserInCloud(args[0], args.Contains("-r"));
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
                if (args.Count < 1)
                {
                    Console.WriteLine("Not enough Arguments");
                }
                CloudRequest.DeleteUserFromCloud(args[0]);
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
                if (args.Count < 3)
                {
                    Console.WriteLine("Not enough Arguments");
                    return;
                }
                await CloudRequest.UploadFile(args[0], args[1], args[2]);
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
                await Account.AddAccount(args);
                break;
            case "update":
                Account.UpdateAccount(args);
                break;
            case "get":
                if (args.Count < 2)
                {
                    Console.WriteLine("Not enough Arguments");
                }
                Account.GetProp(args[0], args[1]);
                break;
            default:
                Console.WriteLine($"{command}: Command Not Found");
                break;
        }
    }
}