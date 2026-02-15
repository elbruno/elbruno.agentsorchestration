Console.WriteLine("Echo Bot - Type 'exit' to quit");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    Console.WriteLine($"Echo: {input}");
}