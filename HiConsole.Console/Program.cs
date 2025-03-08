using MrHihi.HiConsole;

var commandPrompt = new CommandPrompt(enumChatMode.OneLineCommand);

commandPrompt.CommandEnter += (sender, e) =>
{
    Console.WriteLine($"Command: {e.Command}");

    if (e.Command.TrimEnd('\n') == "exit")
    {
        e.Continue = false;
    }
};

commandPrompt.KeyPress += (sender, e) =>
{
    if (e.Key.Key == ConsoleKey.F && e.Key.Modifiers == ConsoleModifiers.Control)
    {
        Console.WriteLine("Ctrl+F is pressed.");
        var p = sender as CommandPrompt;
        p?.SetText("new text\nset to buffer\nthis is a test\n");
        e.Continue = false;
    }
};

commandPrompt.Start("Welcome to HiConsole!", "-> ");
