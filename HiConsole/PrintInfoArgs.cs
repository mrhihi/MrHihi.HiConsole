namespace MrHihi.HiConsole;
public class PrintInfoArgs : EventArgs
{
    public string Info { get; set; } = string.Empty;
    public bool Cancel { get; set; } = false;
}