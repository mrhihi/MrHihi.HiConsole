using System.Text;

namespace MrHihi.HiConsole;
public class TextAreaCoordinate
{
    public int X_CJK => line.ToString(0, X_REL).CJKLength() + _origX;
    public int X_REL => X_ABS - _origX;
    public int X_ABS { get; set; }
    public int Y_ABS { get; set; }
    public int WindowWidth => Console.WindowWidth - _origX - 1;
    private readonly List<StringBuilder> _lines;
    private int _origX;
    private int _origY;
    private int currLineIndex => Y_ABS - _origY;
    private StringBuilder line => _lines[currLineIndex];
    private StringBuilder prevLine => _lines[currLineIndex - 1];
    private StringBuilder nextLine => _lines[currLineIndex + 1];
    private bool isTop => Y_ABS <= _origY;
    private bool isBottom => Y_ABS >= (_origY + _lines.Count - 1);
    private bool isStartOfLine => X_ABS <= _origX;
    private bool isEndOfLine => X_REL >= line.Length;
    private bool hasMoreLines => Y_ABS < (_origY + _lines.Count - 1);
    private readonly Dictionary<ConsoleKey, Action> _keyActions;
    public bool IsInputLineEmpty => line.Length == 0;
    public bool IsAboveLineEmpty => _lines.Count == 1;
    public bool IsAtEnd => isBottom && isEndOfLine;
    public string AboveLine => (_lines.Count > 1) ? _lines[_lines.Count - 2].ToString().TrimEnd('\n') : string.Empty;
    public string AllLine => string.Join('\n', _lines).TrimEnd('\n');
    public string LastLine => _lines.Last().ToString().TrimEnd('\n');

    public TextAreaCoordinate(): this(Console.CursorLeft, Console.CursorTop)
    {
    }
    public TextAreaCoordinate(int x, int y)
    {
        _lines = new List<StringBuilder> { new StringBuilder() };
        _origX = X_ABS = x;
        _origY = Y_ABS = y;
        _keyActions = new Dictionary<ConsoleKey, Action>()
        {
            { ConsoleKey.LeftArrow, MoveLeft },
            { ConsoleKey.RightArrow, MoveRight },
            { ConsoleKey.UpArrow, MoveUp },
            { ConsoleKey.DownArrow, MoveDown },
            { ConsoleKey.Backspace, Backspace },
            { ConsoleKey.Delete, Delete }
        };
    }
    public void Reset()
    {
        _lines.Clear();
        _lines.Add(new StringBuilder());
        X_ABS = _origX = Console.CursorLeft;
        Y_ABS = _origY = Console.CursorTop;
    }
    public bool ProcessKey(ConsoleKeyInfo keyInfo)
    {
        if (!_keyActions.ContainsKey(keyInfo.Key)) return false;
        _keyActions[keyInfo.Key].Invoke();
        return true;
    }

    public void MoveLeft()
    {
        if (!isStartOfLine)
        {
            X_ABS--;
        }
        else if (!isTop)
        {
            Y_ABS--;
            X_ABS = line.Length + _origX;
        }
        DrawCursor();
    }
    public void MoveRight()
    {
        if (!isEndOfLine)
        {
            X_ABS++;
        }
        else if (hasMoreLines)
        {
            Y_ABS++;
            X_ABS = _origX;
        }
        DrawCursor();
    }
    public void MoveUp()
    {
        if (!isTop)
        {
            Y_ABS--;
            X_ABS = Math.Min(X_ABS, line.Length + _origX);
            DrawCursor();
        }
    }
    public void MoveDown()
    {
        if (!isBottom)
        {
            Y_ABS++;
            X_ABS = Math.Min(X_ABS, line.Length + _origX);
            DrawCursor();
        }
    }
    public void RedrawLine(int lineIdx)
    {
        Console.SetCursorPosition(_origX, Y_ABS + lineIdx - currLineIndex);
        if (lineIdx < _lines.Count)
        {
            Console.Write(_lines[lineIdx]);
            for(int j = _lines[lineIdx].CJKLength() - 1; j < WindowWidth; j++)
            {
                Console.Write(' ');
            }
        }
        else
        {
            Console.Write(new string(' ', WindowWidth));
        }
    }
    public void RedrawToEnd(int addition = 0)
    {
        for (int i = currLineIndex; i <= _lines.Count + addition; i++)
        {
            RedrawLine(i);
        }
    }
    public void NewLine()
    {
        string remaining = line.ToString(X_REL, line.Length - X_REL);
        line.Remove(X_REL, line.Length - X_REL);
        _lines.Insert(currLineIndex + 1, new StringBuilder(remaining));
        RedrawToEnd();
        Y_ABS++;
        X_ABS = _origX;
        DrawCursor();
    }
    public void Backspace()
    {
        if (!isStartOfLine)
        {
            line.Remove(X_REL - 1, 1);
            X_ABS--;
            RedrawLine(currLineIndex);
        }
        else if (!isTop)
        {
            // 合併到上一行
            X_ABS = prevLine.Length + _origX;
            prevLine.Append(line.ToString());
            _lines.RemoveAt(currLineIndex);
            Y_ABS--;
            RedrawToEnd(1);
        }
        DrawCursor();
    }
    public void Delete()
    {
        if (!isEndOfLine)
        {
            line.Remove(X_REL, 1);
            RedrawLine(currLineIndex);
        }
        else if (hasMoreLines)
        {
            // 合併下一行
            line.Append(nextLine.ToString());
            _lines.RemoveAt(currLineIndex + 1);
            RedrawToEnd(1);
        }
        DrawCursor();
    }
    public void Insert(char c)
    {
        line.Insert(X_REL, c);
        X_ABS++;
        Console.Write(c);
    }
    public void DrawCursor()
    {
        Console.SetCursorPosition(X_CJK, Y_ABS);
    }
}