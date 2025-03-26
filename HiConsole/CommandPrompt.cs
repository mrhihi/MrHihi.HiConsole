using System.Text;

namespace MrHihi.HiConsole;
public class CommandPrompt
{
    private readonly TextAreaCoordinate _textArea;
    private readonly string _welcome;
    private readonly string _prompt;
    private enumChatMode _mode;
    public TextAreaCoordinate TextArea => _textArea;
    public CommandPrompt(enumChatMode mode, string welcome, string prompt)
    {
        _mode = mode;
        _welcome = welcome;
        _prompt = prompt;
        WriteWelcome();
        WritePrompt();
        _textArea = new TextAreaCoordinate();
        _textArea.CommandInput_TouchTop += (sender, e) => OnCommandInput_TouchTop(e);
        _textArea.CommandInput_TouchBottom += (sender, e) => OnCommandInput_TouchBottom(e);
        _textArea.Console_PrintInfo += (sender, e)=> OnConsole_PrintInfo(e);
    }
    public void WritePrompt()
    {
        Console.Write(_prompt);
    }
    public void WriteWelcome()
    {
       Console.WriteLine(_welcome);
       Console.WriteLine();
    }

    public enumChatMode ChatMode {get { return _mode;}}

    public void ChangeMode(enumChatMode mode)
    {
        _mode = mode;
    }

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

    public event EventHandler<EnterPressArgs>? MultiLineCommand_EnterPress;

    protected virtual void OnMultiLineCommand_EnterPress(EnterPressArgs e)
    {
        MultiLineCommand_EnterPress?.Invoke(this, e);
    }

    public event EventHandler<EnterPressArgs>? OneLineCommand_EnterPress;
    protected virtual void OnOneLineCommand_EnterPress(EnterPressArgs e)
    {
        OneLineCommand_EnterPress?.Invoke(this, e);
    }

    public event EventHandler<KeyPressArgs>? KeyPress;
    
    protected void OnKeyPress(KeyPressArgs e)
    {
        KeyPress?.Invoke(this, e);
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

    protected virtual bool IsTriggerSend(EnterPressArgs e)
    {
        if (_mode == enumChatMode.OneLineCommand)
        {
            e.Triggered = true;
            Console.WriteLine();
            OnOneLineCommand_EnterPress(e);
            if (e.Triggered && !e.Cancel)
            {
                WritePrompt();
                _textArea.Reset();
            }
            return true;
        }
        else if (_textArea.IsAtEnd)
        {
            _textArea.NewLine();
            OnMultiLineCommand_EnterPress(e);
            if (e.Triggered)
            {
                WritePrompt();
                _textArea.Reset();
            }
            return true;
        }
        return false;
    }
    public void MessageLoop()
    {
        _textArea.Loop();
    }
    /// <summary>
    /// Starts the console prompt.
    /// </summary>
    /// <param name="welcome"> Welcome message. </param>
    /// <param name="prompt"> Prompt message. </param>
    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        _textArea.Loop();
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            var kpe = new KeyPressArgs();
            kpe.Key = keyInfo;
            OnKeyPress(kpe);
            if (kpe.Cancel) break; // Stop waiting key read.
            if (kpe.Processed) continue; // Bypass onetime key process.

            if (_textArea.ProcessKey(keyInfo))
            {
                // do nothing
            }
            else if (IsStopeInput(keyInfo))
            {
                break;
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                var e = new EnterPressArgs
                {
                    Command = _textArea.LastLine,
                    Buffer = _textArea.AboveLine
                };
                if (IsTriggerSend(e))
                {
                    if (e.Cancel) break;
                }
                else
                {
                    _textArea.NewLine();
                }
            }
            else if (char.IsControl(keyInfo.KeyChar))
            {
                // do nothing
            }
            else
            {
                _textArea.InsertAsync(keyInfo.KeyChar).Wait();
            }
            if (!Console.KeyAvailable)
            {
                MessageLoop();
                continue;
            }
        }
    }
}