namespace CloudClientConsole;
class Program
{
    public static async Task Main(string[] args)
    {
        await Start();
        //Console.WriteLine(CryptographyTest.TestAes());
        //Console.WriteLine(await CryptographyTest.TestAesStream());
    }

    public static async Task Start()
    {
        if (Account.Accounts.Count == 1) LocalFileSystem.id = Account.Accounts.First().AccId;
        Console.WriteLine("Hello");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (input is null) continue;

            input = RemoveDoubleWhitespace(input);
            if(!input.Any()) continue;
            
            var commands = new List<string>();
            commands.Add(string.Empty);
            foreach (var c in input)
            {
                if (c == ' ')
                {
                    commands.Add(string.Empty);
                    continue;
                }

                commands[^1] += c;
            }

            if (commands[0] == "exit") break;
            
            await CommandManager.ProcessCommand(commands);
        }
    }

    private static string RemoveDoubleWhitespace(string args)
    {
        args = args.Trim();
        for (int i = 0; i < args.Length-1; i++)
        {
            if (args[i] == ' ' && args[i + 1] == ' ')
            {
                args = args.Remove(i, 1);
                i--;
            }
        }
        return args;
    }
}