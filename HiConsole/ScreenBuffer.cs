using System.Text;

namespace MrHihi.HiConsole;
public static class HiExtensions
{
    public static bool IsCJK(this char c)
    {
        return (c >= '\u4E00' && c <= '\u9FFF') ||  // 基本漢字
                (c >= '\u3400' && c <= '\u4DBF');
    }
    public static int CJKLength(this string s)
    {
        return s.Sum(c => c.IsCJK() ? 2 : 1);
    }
    public static string LastLine(this StringBuilder sb)
    {
        if (sb.Length == 0) return string.Empty;
        var lastLine = new StringBuilder();
        for (int i = sb.Length - 1; i >= 0; i--)
        {
            if (sb[i] == '\n') break;
            lastLine.Insert(0, sb[i]);
        }
        return lastLine.ToString();
    }
    public static void RemoveLastLine(this StringBuilder sb)
    {
        while (sb.Length > 0 && sb[sb.Length - 1] != '\n')
        {
            sb.Remove(sb.Length - 1, 1);
        }
    }
    public static void RemoveLastChar(this StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }
    }
    public static bool IsEmpty(this StringBuilder sb)
    {
        return sb.Length == 0;
    }
    public static bool IsEmpty(this string s)
    {
        return s.Length == 0;
    }
    public static string Repeat(this string s, int count)
    {
        return new StringBuilder().Insert(0, s, count).ToString();
    }
}
public class ScreenBuffer
{
    private StringBuilder _buffer = new StringBuilder();
    private StringBuilder _inputLine = new StringBuilder();
    private string _promptString = ">";
    private string _welcomeString = "Welcome to HiConsole!";

    private int promptLength => _promptString.Length;
    public void SetPrompt(string prompt, string welcome)
    {
        _promptString = prompt;
        _welcomeString = welcome;
    }

    public void WritePrompt()
    {
        Console.Write(_promptString);
    }

    public void WriteWelcome()
    {
        Console.WriteLine();
        Console.WriteLine(_welcomeString);
    }

    /// <summary>
    /// Returns true if the input line is empty.
    /// </summary>
    public bool InputLineIsEmpty => _inputLine.IsEmpty();
    /// <summary>
    /// Returns true if the buffer is empty.
    /// </s/// ummary>
    public bool ScreenIsEmpty => _buffer.IsEmpty();

    /// <summary>
    /// Erases the last line's \n character and shows the result on the console.
    /// </summary>
    /// <param name="promptLength"> Prompt string's length. </param>
    public void EraseNewLine()
    {
        int promptLength = _promptString.CJKLength();
        _buffer.RemoveLastChar();
        var lastline = _buffer.LastLine();
        int newlastlinelen = lastline.CJKLength() + promptLength + 1;
        if (!lastline.IsEmpty())
        {
            _inputLine.Append(lastline);
        }
        _buffer.RemoveLastLine();
        // 清掉 prompt 
        Console.Write("\b \b".Repeat(promptLength));
        Console.SetCursorPosition(newlastlinelen - 1, Console.CursorTop - 1);
    }

    /// <summary>
    /// Erases the last character in the input line and shows the result on the console.
    /// </summary>
    public void EraseLastChar()
    {
        if ((_inputLine[_inputLine.Length - 1]).IsCJK())
        {
            Console.Write("\b \b");
        }
        Console.Write("\b \b");
        _inputLine.Remove(_inputLine.Length - 1, 1);
    }

    public bool KeyProcessor(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Backspace)
        {
            if ( _inputLine.IsEmpty() ) {
                if ( !_buffer.IsEmpty() )
                {
                    EraseNewLine();
                }
            }
            else
            {
                EraseLastChar();
            }
            return true;
        } else if (key.Key == ConsoleKey.UpArrow)
            {
                MoveUp(key);
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                MoveDown(key);
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                MoveLeft(key);
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                MoveRight(key);
            }
        return false;
    }

    public void MoveUp(ConsoleKeyInfo key)
    {

    }
    public void MoveDown(ConsoleKeyInfo key)
    {

    }
    public void MoveLeft(ConsoleKeyInfo key)
    {
        if (Console.CursorLeft == 0) return;
        if (_inputLine.IsEmpty()) return;
        int promptLength = _promptString.CJKLength();
        if (Console.CursorLeft <= promptLength) return;
        var lstword = _inputLine[_inputLine.Length - 1];
        var moveCnt = lstword.IsCJK() ? 2 : 1;
        if (Console.CursorLeft >= moveCnt)
        {
            Console.SetCursorPosition(Console.CursorLeft - moveCnt, Console.CursorTop);
        }
    }
    public void MoveRight(ConsoleKeyInfo key)
    {
        if (_inputLine.IsEmpty()) return;
        if (Console.CursorLeft >= Console.BufferWidth) return;
        var lstline = _inputLine.LastLine();
        var len = lstline.CJKLength();
        if (Console.CursorLeft >= len + _promptString.CJKLength() ) return;

        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);

    }
    public Encoding OutputEncoding
    { 
        get { return Console.OutputEncoding; }
        set { Console.OutputEncoding = value; }
    }
    public int CursorLeft => Console.CursorLeft;
    public int CursorTop => Console.CursorTop;

    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        return Console.ReadKey(intercept);
    }

    public void WriteLine(string text = "")
    {
        Console.WriteLine(text);
    }
    public void Write(string text = "")
    {
        Console.Write(text);
    }

    public void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
    }

    /// <summary>
    /// Appends the input line into the buffer and adds a new line.
    /// </summary>
    public void NewLine()
    {
        _buffer.Append(_inputLine);
        _buffer.AppendLine();
        _inputLine.Clear();
        Console.WriteLine();
    }

    public string GetBuffer()
    {
        return _buffer.ToString();
    }

    public string GetInputLine()
    {
        return _inputLine.ToString();
    }

    /// <summary>
    /// Gets the text in the buffer and the input line and resets the input line.
    /// </summary>
    /// <returns></returns>
    public string GetTextAndReset()
    {
        _buffer.Append(_inputLine); // 將最後一行的資料加入
        var text = GetBuffer();
        // Console.WriteLine(" debug:" + text);
        Reset();
        return text;
    }
    /// <summary>
    /// Clears the buffer and input line and resets the cursor position.
    /// </summary>
    public void Reset()
    {
        _buffer.Clear();
        _inputLine.Clear();
    }
    /// <summary>
    /// Appends a character to the input line and displays it on the console.
    /// </summary>
    /// <param name="c"></param>
    public void Append(char c)
    {
        _inputLine.Append(c);
        Console.Write(c);
    }
    /// <summary>
    /// Sets the text of the screen buffer. The input text is split into lines,
    /// with the last line being set as the current input line and the rest
    /// being set as the buffer. The text is then displayed on the console.
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        var t = text.TrimEnd('\n');
        var at = t.Split('\n');
        var l = at.Last();
        _inputLine.Clear();
        _inputLine.Append(l);
        _buffer.Clear();
        var k = at.Take(at.Length - 1);
        if (k.Count() > 0)
        {
            _buffer.Append(string.Join('\n', at.Take(at.Length - 1))+ '\n');
        }
        Console.Write(t);
    }
}