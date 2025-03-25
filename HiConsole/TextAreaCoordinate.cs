using System.Text;

namespace MrHihi.HiConsole;
public class TextAreaCoordinate
{
    public event EventHandler<CommandTouchArgs>? CommandInput_TouchTop;
    protected virtual void OnCommandInput_TouchTop(CommandTouchArgs e)
    {
        CommandInput_TouchTop?.Invoke(this, e);
    }
    public event EventHandler<CommandTouchArgs>? CommandInput_TouchBottom;
    protected virtual void OnCommandInput_TouchBottom(CommandTouchArgs e)
    {
        CommandInput_TouchBottom?.Invoke(this, e);
    }
    public event EventHandler<PrintInfoArgs>? Console_PrintInfo;
    protected virtual void OnConsole_PrintInfo(PrintInfoArgs e)
    {
        Console_PrintInfo?.Invoke(this, e);
    }

    public int X_CJK => LineOp.Line_BeforeCursorToStr().CJKLength() + _origX;
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
    private LineOperator LineOp => new LineOperator(this);

    class LineOperator
    {
        private readonly TextAreaCoordinate _tac;
        public LineOperator(TextAreaCoordinate tac)
        {
            _tac = tac;
        }
        public StringBuilder Line => _tac.line;
        public StringBuilder PrevLine => _tac.prevLine;
        public StringBuilder NextLine => _tac.nextLine;
        public List<StringBuilder> Lines => _tac._lines;
        public void Line_Insert(char c)
        {
            var sidx = Math.Min(_tac.line.Length, _tac.X_REL);
            Line.Insert(sidx, c);
        }
        public string Line_ToString()
        {
            return Line.ToString();
        }
        public string Line_BeforeCursorToStr()
        {
            return Line.ToString(0, Math.Min(Line.Length, _tac.X_REL));
        }
        public string Line_AfterCursorToStr()
        {
            var s = Math.Min(Line.Length, _tac.X_REL);
            return Line.ToString(s, Line.Length - s);
        }
        public string Line_Right(int c)
        {
            if (c <= 0) return string.Empty;
            if (c >= Line.Length) return Line.ToString();
            return Line.ToString(Line.Length - c, c);
        }
        public void Line_RemoveCursorToEnd()
        {
            if (_tac.X_REL >= Line.Length) return;
            if (_tac.X_REL == 0)
            {
                Line.Clear();
                return;
            }
            Line.Remove(_tac.X_REL, Line.Length - _tac.X_REL);
        }
        public void Line_RemoveCursorToCount(int count, int xadj = 0)
        {
            int x_rel = _tac.X_REL + xadj;
            if (count <= 0) return;
            var s = Math.Min(Line.Length, x_rel);
            if (s == Line.Length) return;
            if (count >= Line.Length - s) count = Line.Length - s;
            Line.Remove(s, count);
        }
        public void Line_RemoveRight(int count)
        {
            if (count <= 0) return;
            if (count >= Line.Length) Line.Clear();
            Line.Remove(Line.Length - count, count);
        }
        public void Backspace_TryMergeLines()
        {
            // 如果上一行的空間還夠放，就直接合併到上一行
            if (PrevLine.CJKLength() + Line.CJKLength() <= _tac.WindowWidth)
            {
                var norolling = !_tac.tryRollingUp();
                _tac.X_ABS = PrevLine.Length + _tac._origX;
                PrevLine.Append(Line_ToString());
                Lines.RemoveAt(_tac.cursorLineIdx);
                _tac.Y_ABS--;
                if (norolling)
                {
                    _tac.RedrawToEnd(1);
                }
            }
        }

        public void Line_Delete_TryMergeLines()
        {
            // 如果這一行的空間還夠放下下一行，就直接合併到這一行
            if (Line.CJKLength() + NextLine.CJKLength() <= _tac.WindowWidth)
            {
                Line.Append(NextLine.ToString());
                Lines.RemoveAt(_tac.cursorLineIdx + 1);
                _tac.RedrawToEnd(1);
            }
        }
    }

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
            { ConsoleKey.Delete, Delete },
            { ConsoleKey.Home, MoveHome },
            { ConsoleKey.End, MoveEnd }
        };
    }

    public void ResetLines(string s, bool cursorToEnd = false)
    {
        _lines.Clear();
        s.Split('\n').ToList().ForEach(l => _lines.Add(new StringBuilder(l)));
        if (cursorToEnd)
        {
            Y_ABS = _origY + _lines.Count - 1;
            X_ABS = _origX + line.CJKLength();
        }
        else
        {
            Y_ABS = _origY;
            X_ABS = _origX + line.CJKLength();
        }
        RedrawAll();
        drawCursor();
    }

    public void SetCurrentLine(string s)
    {
        line.Clear();
        line.Append(s);
        ReDrawLine(cursorLineIdx);
        X_ABS = _origX + line.CJKLength();
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
    public void MoveHome()
    {
        if (!isStartOfLine)
        {
            X_ABS = _origX;
            drawCursor();
        }
    }
    public void MoveEnd()
    {
        if (!isEndOfLine)
        {
            X_ABS = line.Length + _origX;
            drawCursor();
        }
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
        if (isTop)
        {
            var e = new CommandTouchArgs();
            e.CurrBuffer = AboveLine;
            e.CurrCommand = LastLine;
            OnCommandInput_TouchTop(e);
            if (e.Setting)
            {
                SetCurrentLine(e.Command);
                drawCursor();
            }
        }
        else
        {
            bool mv = tryRollingUp();
            Y_ABS--;
            X_ABS = Math.Min(X_ABS, line.Length + _origX);
            drawCursor();
        }
    }
    public void MoveDown()
    {
        if (isBottom)
        {
            var e = new CommandTouchArgs();
            e.CurrBuffer = AboveLine;
            e.CurrCommand = LastLine;
            OnCommandInput_TouchBottom(e);
            if (e.Setting)
            {
                SetCurrentLine(e.Command);
                drawCursor();
            }
        }
        else
        {
            tryRollingDown();
            Y_ABS++;
            X_ABS = Math.Min(X_ABS, line.Length + _origX);
            drawCursor();
        }
    }

    public void ReDrawLine(int lineIdx)
    {
        Console.SetCursorPosition(_origX, Y_ABS + lineIdx - cursorLineIdx);
        Console.CursorVisible = false;
        if (lineIdx < _lines.Count)
        {
            var s = _lines[lineIdx];
            int times = WindowWidth - s.CJKLength();
            Console.Write(s + ' '.Repeat(times));
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
            ReDrawLine(i);
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
        string remaining = LineOp.Line_AfterCursorToStr();
        LineOp.Line_RemoveCursorToEnd();
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
            LineOp.Line_RemoveCursorToCount(1, -1);
            X_ABS--;
            ReDrawLine(cursorLineIdx);
            Console.CursorVisible = true;
        }
        else if (!isTop)
        {
            LineOp.Backspace_TryMergeLines();

        }
        drawCursor();
    }
    public void Delete()
    {
        if (!isEndOfLine)
        {
            LineOp.Line_RemoveCursorToCount(1);
            ReDrawLine(cursorLineIdx);
            Console.CursorVisible = true;
        }
        else if (hasMoreLines)
        {
            LineOp.Line_Delete_TryMergeLines();
        }
        drawCursor();
    }

    public async Task InsertAsync(char c)
    {
        await Task.Run(() => {
            if (line.CJKLength() + c.CJKLength() > WindowWidth)
            {
                if (isEndOfLine)
                {
                    NewLine();
                    line.Append(c);
                    X_ABS++;
                    Console.Write(c);
                }
                else
                {
                    LineOp.Line_Insert(c);
                    X_ABS++;
                    var cl = c.CJKLength();
                    var remaining = LineOp.Line_Right(cl);
                    LineOp.Line_RemoveRight(cl);
                    if (hasMoreLines && (_lines[cursorLineIdx + 1].CJKLength() + cl) <= WindowWidth)
                    {
                        _lines[cursorLineIdx + 1].Append(remaining);
                    }
                    else
                    {
                        _lines.Insert(cursorLineIdx + 1, new StringBuilder(remaining));
                    }
                    var y = Y_ABS;
                    RedrawAll();
                    Y_ABS = y;
                    drawCursor();
                }
                return;
            }
            // 在游標所在位置插入字元
            LineOp.Line_Insert(c);
            X_ABS++;
            // 如果游標在行為，就直接寫出來，不然就要把游標後面的字都重寫一次
            if (isEndOfLine)
            {
                Console.Write(c);
            }
            else
            {
                ReDrawLine(cursorLineIdx);
                Console.CursorVisible = true;
                drawCursor();
            }
        });
    }
    private void drawCursor()
    {
        Console.SetCursorPosition(X_CJK, Y_ABS);
    }

    public void PrintInfo(string s)
    {
        var e = new PrintInfoArgs { Info = s };
        OnConsole_PrintInfo(e);
        if (!e.Cancel)
        {
            Print(e.Info, 0, _origY - 1);
        }
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