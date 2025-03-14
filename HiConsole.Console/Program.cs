using MrHihi.HiConsole;

var commandPrompt = new CommandPrompt(enumChatMode.OneLineCommand, "Welcome to HiConsole!", "-> ");

commandPrompt.MultiLineCommand_EnterPress += (sender, e) =>
{
    if (e.Command == "exit")
    {
        e.Cancel = true;
    }
};

commandPrompt.Start();
