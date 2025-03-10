using System.Text;

namespace MrHihi.HiConsole;
public class CommandPrompt
{
    private List<string> _consoleHistory = new List<string>();
    private ScreenBuffer _screen = new ScreenBuffer();

    private enumChatMode _mode;
    public CommandPrompt(enumChatMode mode)
    {
        _mode = mode;
    }
    public enumChatMode ChatMode {get { return _mode;}}

    public void ChangeMode(enumChatMode mode)
    {
        _mode = mode;
    }

    public class BeforeCommandEnterArgs : EventArgs
    {
        public string Buffer { get; set; } = string.Empty;
        public string InputLine { get; set; } = string.Empty;
        public bool TriggerSend { get; set; } = false;
    }
    public event EventHandler<BeforeCommandEnterArgs>? BeforeCommandEnter;
    protected void OnBeforeCommandEnter(BeforeCommandEnterArgs e)
    {
        if (BeforeCommandEnter == null) return;
        e.Buffer = _screen.GetBuffer();
        e.InputLine = _screen.GetInputLine();
        BeforeCommandEnter?.Invoke(this, e);
    }

    public class CommandEnterArgs : EventArgs
    {
        public string Command { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public bool Continue { get; set; } = true;
    }
    public event EventHandler<CommandEnterArgs>? CommandEnter;

    protected void OnCommandEnter(CommandEnterArgs e)
    {
        CommandEnter?.Invoke(this, e);
    }

    public class KeyPressArgs : EventArgs
    {
        public ConsoleKeyInfo Key { get; set; }
        public bool Continue { get; set; } = true;
    }
    public event EventHandler<KeyPressArgs>? KeyPress;
    
    protected void OnKeyPress(KeyPressArgs e)
    {
        KeyPress?.Invoke(this, e);
    }

    /// <summary>
    /// Writes the prompt text into the console.
    /// </summary>
    public void WritePrompt()
    {
        Console.Write(_promptString);
    }
    /// <summary>
    /// Writes the welcome text into the console.
    /// </summary>
    public void WriteWelcome()
    {
        Console.WriteLine(_welcomeString);
    }
    /// <summary>
    /// Sets the text into the console.(buffer and input line will be reset)
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        Console.WriteLine();
        WriteWelcome();
        WritePrompt();
        _screen.SetText(text);
    }
    private void sendCommand(string text, CommandEnterArgs e)
    {
        var t = text.TrimEnd('\n');
        _consoleHistory.Add(t);
        e.Command = t;
        Console.WriteLine();
        OnCommandEnter(e);
        WritePrompt();
    }
    private void debug(string text)
    {
        // 先紀錄 Console 的位置
        int left = Console.CursorLeft;
        int top = Console.CursorTop;

        // 移到第一行的最後面
        Console.SetCursorPosition(0, 0);
        Console.Write(text);
        // 移回原本的位置
        Console.SetCursorPosition(left, top);
    }

    /// <summary>
    /// Returns true if the key is we want to reset the current input.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected virtual bool IsResetCurrentInput(ConsoleKeyInfo key)
    {
        return key.Key == ConsoleKey.E && key.Modifiers == ConsoleModifiers.Control;
    }

    /// <summary>
    /// Returns true if the key is we want to stop the input.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected virtual bool IsStopeInput(ConsoleKeyInfo key)
    {
        return key.Key == ConsoleKey.D && key.Modifiers == ConsoleModifiers.Control;
    }
    /// <summary>
    /// Returns true if the key is we want to start a new line.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected virtual bool IsNewLine(ConsoleKeyInfo key)
    {
        return key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow;
    }

    private string _welcomeString = string.Empty;
    private string _promptString = string.Empty;

    /// <summary>
    /// Starts the console prompt.
    /// </summary>
    /// <param name="welcome"> Welcome message. </param>
    /// <param name="prompt"> Prompt message. </param>
    public void Start(string welcome, string prompt)
    {
        _welcomeString = welcome;
        _promptString = prompt;
        Console.OutputEncoding = Encoding.UTF8;
        ConsoleKeyInfo key;

        WriteWelcome();
        WritePrompt();
        while (true)
        {
            key = Console.ReadKey(true);

            var kpe = new KeyPressArgs();
            kpe.Key = key;
            OnKeyPress(kpe);
            if (!kpe.Continue) continue;

            if (key.Key == ConsoleKey.Backspace)
            {
                if ( _screen.InputLineIsEmpty ) {
                    if ( !_screen.ScreenIsEmpty )
                    {
                        _screen.EraseNewLine(_promptString.Length);
                    }
                    continue;
                }
                else
                {
                    _screen.EraseLastChar();
                }
            }
            else if (IsResetCurrentInput(key))
            {
                _screen.Reset();
                Console.WriteLine();
                WritePrompt();
                continue;
            }
            else if (IsStopeInput(key))
            {
                break;
            }
            else if (IsNewLine(key))
            {
                if (_mode != enumChatMode.Hybrid) continue;
                _screen.NewLine();
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                if (_screen.InputLineIsEmpty) // 最後一行是空的時，按下 Enter 後檢查是否要送出
                {
                    if (_screen.ScreenIsEmpty) continue;

                    var be = new BeforeCommandEnterArgs();
                    be.TriggerSend = true;
                    OnBeforeCommandEnter(be);
                    if (be.TriggerSend)
                    {
                        var e = new CommandEnterArgs();
                        sendCommand(_screen.GetTextAndReset(), e);
                        if (e.Continue) continue;
                        break;
                    }
                }
                else
                {
                    var be = new BeforeCommandEnterArgs();
                    OnBeforeCommandEnter(be);
                    if (be.TriggerSend)
                    {
                        var e = new CommandEnterArgs();
                        e.Trigger = be.InputLine;
                        sendCommand(_screen.GetTextAndReset(), e);
                        if (e.Continue) continue;
                        break;
                    }
                    else
                    {
                        if (_mode == enumChatMode.OneLineCommand || _mode == enumChatMode.Hybrid)
                        {
                            if (_screen.ScreenIsEmpty) // 表示只有當前輸入行有資料
                            {
                                var e = new CommandEnterArgs();
                                sendCommand(_screen.GetTextAndReset(), e);
                                if (e.Continue) continue;
                                break;
                            }
                        }
                        _screen.NewLine();
                    }
                }
            }
            else if (char.IsControl(key.KeyChar))
            {
                continue;
            }
            else
            {
                _screen.Append(key.KeyChar);
            }
        }
    }
}