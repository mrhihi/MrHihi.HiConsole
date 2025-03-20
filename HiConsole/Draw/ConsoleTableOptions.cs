namespace MrHihi.HiConsole.Draw;
public class ConsoleTableOptions
{
    public IEnumerable<string> Columns { get; set; } = new List<string>();
    public bool EnableCount { get; set; } = true;

    /// <summary>
    /// Enable only from a list of objects
    /// </summary>
    public ConsoleTableEnums.Alignment NumberAlignment { get; set; } = ConsoleTableEnums.Alignment.Left;
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// The <see cref="TextWriter"/> to write to. Defaults to <see cref="Console.Out"/>.
    /// </summary>
    public TextWriter OutputTo { get; set; } = Console.Out;
    public static ConsoleTableOptions Default => new ConsoleTableOptions();
    public ConsoleTableOptions WithOutputTo(TextWriter writer)
    {
        OutputTo = writer;
        return this;
    }
}

