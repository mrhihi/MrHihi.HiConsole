namespace MrHihi.HiConsole;
public class KeyPressArgs : EventArgs
{
    public ConsoleKeyInfo Key { get; set; }
    /// <summary>
    /// Set to true: Bypass onetime key process.
    /// </summary>
    /// <value></value>
    public bool Processed { get; set; } = false;
    /// <summary>
    /// Set to true: Stope waitting key read.
    /// </summary>
    /// <value></value>
    public bool Cancel { get; set; } = false;
}