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
    public static string Repeat(this char c, int times)
    {
        if (times <= 0) return string.Empty;
        return new string(c, times);
    }
}