namespace MrHihi.HiConsole;
public class CommandTouchArgs : EventArgs
{
    public bool Setting { get; set; } = false;
    public string Command { get; set; } = string.Empty;
}