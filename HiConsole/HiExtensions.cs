using System.Text;

namespace MrHihi.HiConsole;
public static class HiExtensions
{
    public static bool IsCJK(this char c)
    {
        return (c >= '\u3000' && c <= '\u9FFF') ||
               (c >= '\uA000' && c <= '\uD7FF') ||
               (c >= '\uF900' && c <= '\uFFFD')
        ;
    }
    public static int CJKLength(this char c)
    {
        return c.IsCJK() ? 2 : 1;
    }
    public static int CJKLength(this string s)
    {
        return s.Sum(c => c.IsCJK() ? 2 : 1);
    }
    public static int CJKLength(this StringBuilder sb)
    {
        if (sb.Length == 0) return 0;
        return sb.ToString().CJKLength();
    }
    public static string CJKSubString(this StringBuilder s, int startIndex, int length)
    {
        if (s.Length == 0) return string.Empty;
        if (startIndex < 0) startIndex = 0;
        if (length <= 0) return string.Empty;
        if (startIndex >= s.Length) return string.Empty;
        if (startIndex + length > s.Length) length = s.Length - startIndex;
        var sb = new StringBuilder();
        int clen = 0;
        for (int i = 0; i < length; i++)
        {
            var c = s[startIndex + i];
            clen += c.CJKLength();
            if (clen > length) break;
            sb.Append(c);
        }
        return sb.ToString();
    }
    public static string Repeat(this char c, int times)
    {
        if (times <= 0) return string.Empty;
        return new string(c, times);
    }
}