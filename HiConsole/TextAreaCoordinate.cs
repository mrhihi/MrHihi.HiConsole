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
    private int cursorLineIdx => Y_ABS - _origY;
    private StringBuilder line => _lines[cursorLineIdx];
    private StringBuilder prevLine => _lines[cursorLineIdx - 1];
    private StringBuilder nextLine => _lines[cursorLineIdx + 1];
    private bool isTop => Y_ABS <= _origY;
    private bool isBottom => Y_ABS >= (_origY + _lines.Count - 1);
    private bool isStartOfLine => X_ABS <= _origX;
    private bool isEndOfLine => X_REL >= line.Length;
    private bool hasMoreLines => Y_ABS < (_origY + _lines.Count - 1);
    private readonly Dictionary<ConsoleKey, Action> _keyActions;
    private int scrollingRows = 0;
    public bool IsInputLineEmpty => line.Length == 0;
    public bool IsAboveLineEmpty => _lines.Count == 1;
    public bool IsAtEnd => isBottom && isEndOfLine;
    public string AboveLine => (_lines.Count > 1) ? string.Join('\n', _lines.Take(_lines.Count - 1)).TrimEnd('\n') : string.Empty;
    public string AllLine => string.Join('\n', _lines).TrimEnd('\n');
    public string LastLine => _lines.Last().ToString().TrimEnd('\n');

    public TextAreaCoordinate(): this(Console.CursorLeft, Console.CursorTop) {}
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
        drawCursor();
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
        drawCursor();
    }
    public void MoveUp()
    {
        if (!isTop)
        {
            bool mv = tryRollingUp();
            Y_ABS--;
            X_ABS = Math.Min(X_ABS, line.Length + _origX);
            drawCursor();
        }
    }
    public void MoveDown()
    {
        if (!isBottom)
        {
            tryRollingDown();
            Y_ABS++;
            X_ABS = Math.Min(X_ABS, line.Length + _origX);
            drawCursor();
        }
    }
    public void RedrawLine(int lineIdx)
    {
        Console.SetCursorPosition(_origX, Y_ABS + lineIdx - cursorLineIdx);
        Console.CursorVisible = false;
        if (lineIdx < _lines.Count)
        {
            int times = WindowWidth - _lines[lineIdx].CJKLength();
            Console.Write(_lines[lineIdx] + ' '.Repeat(times));
        }
        else
        {
            Console.Write(' '.Repeat(WindowWidth));
        }
    }
    public void RedrawToEnd(int addition = 0)
    {
        for (int i = cursorLineIdx; i <= _lines.Count + addition; i++)
        {
            RedrawLine(i);
        }
        Console.CursorVisible = true;
    }
    public void RedrawLine2(int lineIdx)
    {
        Console.SetCursorPosition(_origX, _origY + lineIdx);
        Console.CursorVisible = false;
        var l = _lines[Math.Min(_lines.Count - 1, lineIdx + scrollingRows)];
        int times = WindowWidth - l.CJKLength();
        Console.Write(l + ' '.Repeat(times));
    }
    public void RedrawAll()
    {
        var end = Math.Min(_lines.Count, Console.WindowHeight - _origY);
        for (int i = 0; i < end; i++)
        {
            RedrawLine2(i);
        }
        Console.CursorVisible = true;
    }
    public void NewLine()
    {
        bool ib = isBottom;
        string remaining = line.ToString(X_REL, line.Length - X_REL);
        line.Remove(X_REL, line.Length - X_REL);
        _lines.Insert(cursorLineIdx + 1, new StringBuilder(remaining));
        if (!tryRollingDown())
        {
            RedrawToEnd();
        }
        Y_ABS++;
        X_ABS = _origX;
        drawCursor();
    }
    public void Backspace()
    {
        if (!isStartOfLine)
        {
            line.Remove(X_REL - 1, 1);
            X_ABS--;
            RedrawLine(cursorLineIdx);
            Console.CursorVisible = true;
        }
        else if (!isTop)
        {
            var norolling = !tryRollingUp();
            // 合併到上一行
            X_ABS = prevLine.Length + _origX;
            prevLine.Append(line.ToString());
            _lines.RemoveAt(cursorLineIdx);
            Y_ABS--;
            if (norolling)
            {
                RedrawToEnd(1);
            }
        }
        drawCursor();
    }
    public void Delete()
    {
        if (!isEndOfLine)
        {
            line.Remove(X_REL, 1);
            RedrawLine(cursorLineIdx);
            Console.CursorVisible = true;
        }
        else if (hasMoreLines)
        {
            // 合併下一行
            line.Append(nextLine.ToString());
            _lines.RemoveAt(cursorLineIdx + 1);
            RedrawToEnd(1);
        }
        drawCursor();
    }
    public void Insert(char c)
    {
        line.Insert(X_REL, c);
        X_ABS++;
        if (isEndOfLine)
        {
            Console.Write(c);
        }
        else
        {
            RedrawLine(cursorLineIdx);
            Console.CursorVisible = true;
            drawCursor();
        }
    }
    private void drawCursor()
    {
        Console.SetCursorPosition(X_CJK, Y_ABS);
    }

    public void PrintInfo(string s)
    {
        // var x = Console.WindowWidth - s.Length - 1;
        Print(s, 0, _origY - 1);
    }

    public void Print(string s, int px = 0, int py = 0)
    {
        var x = Console.CursorLeft;
        var y = Console.CursorTop;
        Console.CursorVisible = false;
        Console.SetCursorPosition(px, py);
        Console.Write(' '.Repeat(Console.WindowWidth - py));
        Console.SetCursorPosition(px, py);
        Console.Write(s);
        Console.SetCursorPosition(x, y);
        Console.CursorVisible = true;
    }
    public void Loop()
    {
        PrintInfo($"L:{cursorLineIdx} C:{X_REL}");
        // debugDraw($"X_ABS: {X_ABS}, Y_ABS: {Y_ABS}, " +
        //                 $"_origX: {_origX}, _origY: {_origY}, " +
        //                 $"W/SW: {Console.WindowHeight}/{scrollingRows}, " +
        //                 $"cli: {cursorLineIdx}, " +
        //                 $"lcnt: {_lines.Count}, "

        // , py: 0);
        // debugDraw($"isBottom:{isBottom}, scrollingRows:{scrollingRows}", py: 1);
    }

    private bool tryRollingUp()
    {
        var rolling = calcScrollingRows(-1);
        if (rolling)
        {
            RedrawAll();
        }
        return rolling;
    }

    private bool tryRollingDown()
    {
        var rolling = calcScrollingRows(1);
        if (rolling)
        {
            RedrawAll();
        }
        return rolling;
    }

    private bool calcScrollingRows(int predict = 0)
    {
        int origRollingRows = scrollingRows;
        // 當游標超過視窗底部時，計算需要滾動的行數
        if (Y_ABS + predict >= Console.WindowHeight)
        {
            scrollingRows = Y_ABS + predict - Console.WindowHeight + 1;
        }
        else
        {
            scrollingRows = 0;
        }
        return origRollingRows != scrollingRows;
    }

}