using MrHihi.HiConsole;

namespace MrHihi.HiConsole.Test;
public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var commandPrompt = new CommandPrompt(enumChatMode.OneLineCommand);
        commandPrompt.CommandEnter += (sender, e) =>
        {
            var cmd = e.Command.ToLower().Trim();
            Console.WriteLine($"Command: {cmd}");
            if (cmd == "exit")
            {
                e.Continue = false;
            }
            else if (cmd.StartsWith("/mode"))
            {
                var mode = cmd.Substring(5).Trim();
                Console.WriteLine($"Input Mode: `{mode}`");
                if (mode == "onelinecommand")
                {
                    commandPrompt.ChangeMode(enumChatMode.OneLineCommand);
                }
                else if (mode == "multilinecommand")
                {
                    commandPrompt.ChangeMode(enumChatMode.MultiLineCommand);
                }
                else if (mode == "hybrid")
                {
                    commandPrompt.ChangeMode(enumChatMode.Hybrid);
                }
                else
                {
                    Console.WriteLine("Invalid mode.");
                    Console.WriteLine("Available modes: OneLineCommand, MultiLineCommand, Hybrid");
                }
            }
        };
        commandPrompt.Start("Welcome to HiConsole!", "-> ");
    }
}