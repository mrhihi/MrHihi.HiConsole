namespace MrHihi.HiConsole;
public class EnterPressArgs : EventArgs
{
    public required Action<Action> WriteResult { get; set; }
    public string Command { get; set; } = string.Empty;
    public string Buffer { get; set; } = string.Empty;
    public bool Triggered { get; set; } = false;
    public bool Cancel { get; set; } = false;
}