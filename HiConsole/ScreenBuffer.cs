using System.Text;

namespace MrHihi.HiConsole;
public class ScreenBuffer
{
    private StringBuilder _buffer = new StringBuilder();
    private StringBuilder _inputLine = new StringBuilder();
    private static bool isCKJ(char c)
    {
        return (c >= '\u4E00' && c <= '\u9FFF') ||  // 基本漢字
                (c >= '\u3400' && c <= '\u4DBF');
    }

    /// <summary>
    /// Returns true if the input line is empty.
    /// </summary>
    public bool InputLineIsEmpty => _inputLine.Length == 0;
    /// <summary>
    /// Returns true if the buffer is empty.
    /// </summary>
    public bool ScreenIsEmpty => _buffer.Length == 0;

    /// <summary>
    /// Erases the last line's \n character and shows the result on the console.
    /// </summary>
    /// <param name="promptLength"> Prompt string's length. </param>
    public void EraseNewLine(int promptLength)
    {
        _buffer.Remove(_buffer.Length - 1, 1);
        // 計算 textArea 最後一行的長度
        var allLines = _buffer.ToString().Split('\n');
        int lastLineLength = allLines.Last().Sum(c => isCKJ(c) ? 2 : 1) + ((allLines.Length>1)?0:promptLength);
        _inputLine.Append(allLines.Last());
        // 移除 textArea 最後一行
        _buffer.Remove(_buffer.Length - allLines.Last().Length, allLines.Last().Length);
        // 移到上一行的最後一個字
        Console.SetCursorPosition(lastLineLength, Console.CursorTop - 1);
    }

    /// <summary>
    /// Erases the last character in the input line and shows the result on the console.
    /// </summary>
    public void EraseLastChar()
    {
        if (isCKJ(_inputLine[_inputLine.Length - 1]))
        {
            Console.Write("\b \b");
        }
        Console.Write("\b \b");
        _inputLine.Remove(_inputLine.Length - 1, 1);
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

    /// <summary>
    /// Gets the text in the buffer and the input line and resets the input line.
    /// </summary>
    /// <returns></returns>
    public string GetTextAndReset()
    {
        _buffer.Append(_inputLine); // 將最後一行的資料加入
        var text = _buffer.ToString();
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