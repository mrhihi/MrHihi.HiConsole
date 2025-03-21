namespace MrHihi.HiConsole;
public class CommandTouchArgs : EventArgs
{
    public string CurrBuffer { get; internal set; } = string.Empty;
    public string CurrCommand { get; internal set; } = string.Empty;
    public bool Setting { get; set; } = false;
    public string Command { get; set; } = string.Empty;
}