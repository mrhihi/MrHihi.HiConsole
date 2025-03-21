using System.Drawing;
using MrHihi.HiConsole.Draw;

namespace MrHihi.HiConsole.Test;
public class UnitTest1
{
    [Fact]
    public void TestOneLineCommand()
    {
        startCommandPrompt(enumChatMode.OneLineCommand);
    }

    [Fact]
    public void TestMultiLineCommand()
    {
        startCommandPrompt(enumChatMode.MultiLineCommand);
    }

    private void startCommandPrompt(enumChatMode mode)
    {
        var commandPrompt = new CommandPrompt(mode, "Welcome to HiConsole!", "-> ");;
        commandPrompt.OneLineCommand_EnterPress += (sender, e) =>
        {
            var cmd = e.Command.ToLower().Trim();
            if (cmd == "exit")
            {
                e.Cancel = true;
                Console.WriteLine("Bye-bye!");
            }
            else if (cmd == "play")
            {
                Console.WriteLine("Playing ....");
            }
        };
        commandPrompt.MultiLineCommand_EnterPress += (sender, e) =>
        {
            var cmd = e.Command.ToLower().Trim();
            if (cmd == "/exit")
            {
                Console.WriteLine("Bye-bye!");
                e.Cancel = true;
            }
            else if (cmd == "/play")
            {
                Console.WriteLine("Trigger: /play");
                e.Triggered = true;
            }
            else if (cmd == "/resetall to end")
            {
                commandPrompt.TextArea.ResetLines("4\n3\n2\n  1\n", true);
            }
            else if (cmd == "/resetall")
            {
                commandPrompt.TextArea.ResetLines("1\n2\n3\n  4\n");
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
                else
                {
                    Console.WriteLine("Invalid mode.");
                    Console.WriteLine("Available modes: OneLineCommand, MultiLineCommand");
                }
                e.Triggered = true;
            }
        };
        commandPrompt.Start();
    }

    [Fact]
    public void TestDrawTable()
    {
        var content = new List<dynamic>
        {
            new Dictionary<string, object> { 
                { "Name", "MrHihi 中文搞1笑" }, { "Age", 18 }, { "Time", DateTime.Now },
            },
            new Dictionary<string, object> { 
                { "Name", "MrHihi" }, { "Age", 18 }, { "Time", DateTime.Now }
            }
        };
        Draw.ConsoleTable.Print(content);
    }

    [Fact]
    public void TestDrawColor()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                var color = (ConsoleColor)i;
                var bgColor = (ConsoleColor)j;
                Console.WriteLine($" Color {color.ToString()} on {bgColor.ToString()} ".Color(color, bgColor) + " Normal!");
            }
        }
        Console.WriteLine(" Bold ".Bold() + " Normal!");
        Console.WriteLine(" Underline ".Underline() + " Normal!");
        Console.WriteLine(" Reverse ".Color(ConsoleColor.Blue).Reverse() + " Normal!");
    }
}