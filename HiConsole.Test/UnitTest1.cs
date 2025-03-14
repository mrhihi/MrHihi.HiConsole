using MrHihi.HiConsole;

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
                e.WriteResult(()=>{
                    Console.WriteLine("Bye-bye!");
                });
            }
            else if (cmd == "play")
            {
                e.WriteResult(()=>{
                    Console.WriteLine("Playing ....");
                });
            }
        };
        commandPrompt.MultiLineCommand_EnterPress += (sender, e) =>
        {
            var cmd = e.Command.ToLower().Trim();
            if (cmd == "/exit")
            {
                e.WriteResult(() => {
                    Console.WriteLine("Bye-bye!");
                });

                e.Cancel = true;
            }
            else if (cmd == "/play")
            {
                e.WriteResult(() => {
                    Console.WriteLine("Trigger: /play");
                });

                e.Triggered = true;
            }
            else if (cmd.StartsWith("/mode"))
            {
                var mode = cmd.Substring(5).Trim();
                e.WriteResult(() => {
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
                });
                e.Triggered = true;
            }
        };
        commandPrompt.Start();
    }
}