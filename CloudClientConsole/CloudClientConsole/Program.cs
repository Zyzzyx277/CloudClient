using CloudClientConsole;
using CloudClientConsole.tests;

namespace CloudClientConsole;
class Program
{
    public static async Task Main(string[] args)
    {
        await Start();
        //Console.WriteLine(CryptographyTest.TestAes());
    }

    public static async Task Start()
    {
        Console.WriteLine("Hello");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (input is null) continue;

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
}