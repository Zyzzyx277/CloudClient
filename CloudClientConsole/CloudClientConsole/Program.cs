using CloudClientConsole;

Console.WriteLine("Hello");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if(input is null) continue;

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

    string command = commands[0];
    commands.RemoveAt(0);
    switch (command.ToLower())
    {
        case "acc":
            await Account.AccCommands(commands);
            break;
        case "req":
            await CloudRequest.RequestCommand(commands);
            break;
        default:
            Console.WriteLine($"{command}: Command Not Found");
            break;
    }
}