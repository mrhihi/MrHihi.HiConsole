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

    /// <summary>
    /// only works when the mode is MultiLineCommand or Hybrid
    /// </summary>
    /// <value>true: allow empty line / false: disallow empty line</value>
    public bool AllowEmptyLine { get; set; } = true;

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
    /// Sets the text into the console.(buffer and input line will be reset)
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        _screen.WriteWelcome();
        _screen.WritePrompt();
        _screen.SetText(text);
    }
    private void sendCommand(string text, CommandEnterArgs e)
    {
        var t = text.TrimEnd('\n');
        _consoleHistory.Add(t);
        e.Command = t;
        _screen.WriteLine();
        OnCommandEnter(e);
        _screen.WritePrompt();
    }
    private void debug(string text)
    {
        // 先紀錄 Console 的位置
        int left = _screen.CursorLeft;
        int top = _screen.CursorTop;

        // 移到第一行的最後面
        _screen.SetCursorPosition(0, 0);
        _screen.Write(text);
        // 移回原本的位置
        _screen.SetCursorPosition(left, top);
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
    /// Starts the console prompt.
    /// </summary>
    /// <param name="welcome"> Welcome message. </param>
    /// <param name="prompt"> Prompt message. </param>
    public void Start(string welcome, string prompt)
    {
        _screen.SetPrompt(prompt, welcome);
        _screen.OutputEncoding = Encoding.UTF8;
        ConsoleKeyInfo key;

        _screen.WriteWelcome();
        _screen.WritePrompt();
        while (true)
        {
            key = _screen.ReadKey(true);

            var kpe = new KeyPressArgs();
            kpe.Key = key;
            OnKeyPress(kpe);
            if (!kpe.Continue) continue;

            if (_screen.KeyProcessor(key))
            {
                continue;
            }
            else if (IsStopeInput(key))
            {
                break;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                if (_screen.InputLineIsEmpty) // 最後一行是空的時，按下 Enter 後檢查是否要送出
                {
                    if (_screen.ScreenIsEmpty)
                    {
                        if (AllowEmptyLine && _mode != enumChatMode.OneLineCommand)
                        {
                            _screen.NewLine();
                            _screen.WritePrompt();
                        }
                        continue;
                    }
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
                    else
                    {
                        if (AllowEmptyLine && _mode != enumChatMode.OneLineCommand)
                        {
                            _screen.NewLine();
                            _screen.WritePrompt();
                        }
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
                        _screen.WritePrompt();
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