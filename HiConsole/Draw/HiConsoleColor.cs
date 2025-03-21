using System.Text.RegularExpressions;

namespace MrHihi.HiConsole.Draw;
public static class HiConsoleColor
{
    private static string RESET { get; } = Console.IsOutputRedirected ? "" : "\x1b[0m";
    private static string BOLD { get; } = Console.IsOutputRedirected ? "" : "\x1b[1m";
    private static string NOBOLD { get; } = Console.IsOutputRedirected ? "" : "\x1b[22m";
    private static string UNDERLINE { get; } = Console.IsOutputRedirected ? "" : "\x1b[4m";
    private static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    private static string REVERSE { get; } = Console.IsOutputRedirected ? "" : "\x1b[7m";
    private static string NOREVERSE { get; } = Console.IsOutputRedirected ? "" : "\x1b[27m";

    private static Dictionary<ConsoleColor, string> _colorMap = new Dictionary<ConsoleColor, string>
    {
        { ConsoleColor.Black, "{3}0" },
        { ConsoleColor.DarkBlue, "{3}4" },
        { ConsoleColor.DarkGreen, "{3}2" },
        { ConsoleColor.DarkCyan, "{3}6" },
        { ConsoleColor.DarkRed, "{3}1" },
        { ConsoleColor.DarkMagenta, "{3}5" },
        { ConsoleColor.DarkYellow, "{3}3" },
        { ConsoleColor.Gray, "1;{3}0" },
        { ConsoleColor.DarkGray, "1;{3}0" },
        { ConsoleColor.Blue, "1;{3}4" },
        { ConsoleColor.Green, "1;{3}2" },
        { ConsoleColor.Cyan, "1;{3}6" },
        { ConsoleColor.Red, "1;{3}1" },
        { ConsoleColor.Magenta, "1;{3}5" },
        { ConsoleColor.Yellow, "1;{3}3" },
        { ConsoleColor.White, "1;{3}7" }
    };

    /// <summary>
    /// 將文字填充 Console 色彩
    /// </summary>
    /// <param name="color">顏色</param>
    /// <returns></returns>
    public static string Color(this string input, ConsoleColor color, ConsoleColor? baColor = null)
    {
        return $"{GetColor(color, baColor)}{input}{RESET}";
    }

    public static string Reset(this string input)
    {
        return $"{input}{RESET}";
    }

    public static string Underline(this string input)
    {
        return $"{UNDERLINE}{input}{NOUNDERLINE}";
    }
    public static string Bold(this string input)
    {
        return $"{BOLD}{input}{NOBOLD}";
    }
    public static string Reverse(this string input)
    {
        return $"{REVERSE}{input}{NOREVERSE}";
    }
    public static string GetColor(ConsoleColor color, ConsoleColor? baColor = null)
    {
        if(Console.IsOutputRedirected) return "";
        if (baColor == null) return $"\x1b[{_colorMap[color].Replace("{3}", "3")}m";
        return $"\x1b[{_colorMap[color].Replace("{3}", "3")};{_colorMap[baColor??ConsoleColor.Black].Replace("{3}", "4")}m";
    }

    public static string RemvoeAsciiControl(this string input)
    {
        var reg = new Regex(@"\x1B\[[0-?]*[ -/]*[@-~]");
        return reg.Replace(input, "");
    }
}