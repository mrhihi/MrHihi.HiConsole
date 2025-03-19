// reference: https://github.com/khalidabuhakmeh/ConsoleTables/tree/main
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MrHihi.HiConsole.Text;
public class ConsoleTable
{
    static class Symbol
    {
        public const string LINE = "-";
        public const string VLINE = "│";
    }

    public IList<object> Columns { get; }
    public IList<object[]> Rows { get; }

    public ConsoleTableOptions Options { get; }
    public Type[]? ColumnTypes { get; private set; }

    public IList<string> Formats { get; private set; }

    public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
    {
        typeof(int),  typeof(double),  typeof(decimal),
        typeof(long), typeof(short),   typeof(sbyte),
        typeof(byte), typeof(ulong),   typeof(ushort),
        typeof(uint), typeof(float)
    };

    public ConsoleTable(params string[] columns)
        : this(ConsoleTableOptions.Default)
    {
    }

    public ConsoleTable(ConsoleTableOptions options)
    {
        Options = options ?? throw new ArgumentNullException("options");
        Rows = new List<object[]>();
        Columns = new List<object>(options.Columns);
        Formats = new List<string>();
    }

    public ConsoleTable AddColumn(IEnumerable<string> names)
    {
        foreach (var name in names)
            Columns.Add(name);
        return this;
    }

    public ConsoleTable AddRow(params object[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        if (!Columns.Any())
            throw new Exception("Please set the columns first");

        if (Columns.Count != values.Length)
            throw new Exception(
                $"The number columns in the row ({Columns.Count}) does not match the values ({values.Length})");

        Rows.Add(values);
        return this;
    }

    public ConsoleTable Configure(Action<ConsoleTableOptions> action)
    {
        action(Options);
        return this;
    }


    public static ConsoleTable FromDictionary(Dictionary<string, Dictionary<string, object>> values)
    {
        var table = new ConsoleTable();

        var columNames = values.SelectMany(x => x.Value.Keys).Distinct().ToList();
        columNames.Insert(0, "");
        table.AddColumn(columNames);
        foreach (var row in values)
        {
            var r = new List<object> { row.Key };
            foreach (var columName in columNames.Skip(1))
            {
                r.Add(row.Value.TryGetValue(columName, out var value) ? value : "");
            }

            table.AddRow(r.Cast<object>().ToArray());
        }

        return table;
    }

    public static ConsoleTable From<T>(IEnumerable<T> values)
    {
        var table = new ConsoleTable
        {
            ColumnTypes = GetColumnsType<T>().ToArray()
        };

        var columns = GetColumns<T>().ToList();

        table.AddColumn(columns);

        foreach (
            var propertyValues
            in values.Where(x=> x!= null).Select(value => columns.Select(column => GetColumnValue<T>(value??new object(), column)))
        ) table.AddRow(propertyValues.ToArray());

        return table;
    }

    public static ConsoleTable From(DataTable dataTable)
    {
        var table = new ConsoleTable();

        var columns = dataTable.Columns
            .Cast<DataColumn>()
            .Select(x => x.ColumnName)
            .ToList();

        table.AddColumn(columns);

        foreach (DataRow row in dataTable.Rows)
        {
            var items = row.ItemArray.Select(x => x is byte[] data ? Convert.ToBase64String(data)??"" : x?.ToString()??"")
                .ToArray();
            if (items != null && items.Length > 0)
            {
                table.AddRow(items);
            }
        }

        return table;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        // find the longest column by searching each row
        var columnLengths = ColumnLengths();

        // set right alinment if is a number
        var columnAlignment = Enumerable.Range(0, Columns.Count)
            .Select(GetNumberAlignment)
            .ToList();

        // create the string format with padding ; just use for maxRowLength
        var format = Enumerable.Range(0, Columns.Count)
            .Select(i => " " +  Symbol.VLINE + " {" + i + "," + columnAlignment[i] + columnLengths[i] + "}")
            .Aggregate((s, a) => s + a) + " " + Symbol.VLINE;

        SetFormats(ColumnLengths(), columnAlignment);

        // find the longest formatted line
        var maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
        var columnHeaders = string.Format(Formats[0], Columns.ToArray());

        // longest line is greater of formatted columnHeader and longest row
        var longestLine = Math.Max(maxRowLength, columnHeaders.Length);

        // add each row
        var results = Rows.Select((row, i) => string.Format(Formats[i + 1], row)).ToList();

        // create the divider
        var divider = " " + string.Join("", Enumerable.Repeat(Symbol.LINE, longestLine - 1)) + " ";

        builder.AppendLine(divider);
        builder.AppendLine(columnHeaders);

        foreach (var row in results)
        {
            builder.AppendLine(divider);
            builder.AppendLine(row);
        }

        builder.AppendLine(divider);

        if (Options.EnableCount)
        {
            builder.AppendLine("");
            builder.AppendFormat(" Count: {0}", Rows.Count);
        }

        return builder.ToString();
    }


    private void SetFormats(List<int> columnLengths, List<string> columnAlignment)
    {
        var allLines = new List<object[]>();
        allLines.Add(Columns.ToArray());
        allLines.AddRange(Rows);

        Formats = allLines.Select(d =>
        {
            return Enumerable.Range(0, Columns.Count)
                .Select(i =>
                {
                    var value = d[i]?.ToString() ?? "";
                    var length = columnLengths[i] - (GetTextWidth(value) - value.Length);
                    return " " + Symbol.VLINE + " {" + i + "," + columnAlignment[i] + length + "}";
                })
                .Aggregate((s, a) => s + a) + " " + Symbol.VLINE;
        }).ToList();
    }

    public static int GetTextWidth(string value)
    {
        if (value == null)
            return 0;

        var length = value.ToCharArray().Sum(c => c > 127 ? 2 : 1);
        return length;
    }

    public string ToMarkDownString()
    {
        return ToMarkDownString(Symbol.VLINE[0]);
    }

    private string ToMarkDownString(char delimiter)
    {
        var builder = new StringBuilder();

        // find the longest column by searching each row
        var columnLengths = ColumnLengths();

        // create the string format with padding
        _ = Format(columnLengths, delimiter);

        // find the longest formatted line
        var columnHeaders = string.Format(Formats[0].TrimStart(), Columns.ToArray());

        // add each row
        var results = Rows.Select((row, i) => string.Format(Formats[i + 1].TrimStart(), row)).ToList();

        // create the divider
        var divider = Regex.Replace(columnHeaders, "[^"+ Symbol.VLINE +"]", Symbol.LINE);

        builder.AppendLine(columnHeaders);
        builder.AppendLine(divider);
        results.ForEach(row => builder.AppendLine(row));

        return builder.ToString();
    }

    public string ToMinimalString()
    {
        return ToMarkDownString(char.MinValue);
    }

    public string ToStringAlternative()
    {
        var builder = new StringBuilder();

        // find the longest formatted line
        var columnHeaders = string.Format(Formats[0].TrimStart(), Columns.ToArray());

        // add each row
        var results = Rows.Select((row, i) => string.Format(Formats[i + 1].TrimStart(), row)).ToList();

        // create the divider
        var divider = Regex.Replace(columnHeaders, "[^"+ Symbol.VLINE +"]", "─");
        var dividerPlus = divider.Remove(0, 1).Insert(0, "├")
                                .Remove(divider.Length - 1, 1).Insert(divider.Length - 1, "┤")
                                .Replace(Symbol.VLINE, "┼");
        var dividerTop = dividerPlus.Remove(0, 1).Insert(0, "┌")
                                .Remove(dividerPlus.Length - 1, 1).Insert(dividerPlus.Length - 1, "┐")
                                .Replace("┼", "┬");
        var dividerBottom = dividerPlus.Remove(0, 1).Insert(0, "└")
                                .Remove(dividerPlus.Length - 1, 1).Insert(dividerPlus.Length - 1, "┘")
                                .Replace("┼", "┴");
        builder.AppendLine(dividerTop);
        builder.AppendLine(columnHeaders);

        foreach (var row in results)
        {
            builder.AppendLine(dividerPlus);
            builder.AppendLine(row);
        }
        builder.AppendLine(dividerBottom);

        return builder.ToString();
    }


    private string Format(List<int> columnLengths, char delimiter)
    {
        // set right alignment if is a number
        var columnAlignment = Enumerable.Range(0, Columns.Count)
            .Select(GetNumberAlignment)
            .ToList();

        SetFormats(columnLengths, columnAlignment);

        var delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
        var format = (Enumerable.Range(0, Columns.Count)
            .Select(i => " " + delimiterStr + " {" + i + "," + columnAlignment[i] + columnLengths[i] + "}")
            .Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();
        return format;
    }

    private string GetNumberAlignment(int i)
    {
        return Options.NumberAlignment == ConsoleTableEnums.Alignment.Right
                && ColumnTypes != null
                && NumericTypes.Contains(ColumnTypes[i])
            ? ""
            : Symbol.LINE;
    }

    private List<int> ColumnLengths()
    {
        var columnLengths = Columns
            .Select((t, i) => Rows.Select(x => x[i])
                .Union(new[] { Columns[i] })
                .Where(x => x != null)
                .Select(x => (x?.ToString()??"").ToCharArray().Sum(c => c > 127 ? 2 : 1)).Max())
            .ToList();
        return columnLengths;
    }

    public void Write(ConsoleTableEnums.Format format = ConsoleTableEnums.Format.Default)
    {
        SetFormats(ColumnLengths(), Enumerable.Range(0, Columns.Count).Select(GetNumberAlignment).ToList());

        switch (format)
        {
            case ConsoleTableEnums.Format.Default:
                Options.OutputTo.WriteLine(ToString());
                break;
            case ConsoleTableEnums.Format.MarkDown:
                Options.OutputTo.WriteLine(ToMarkDownString());
                break;
            case ConsoleTableEnums.Format.Alternative:
                Options.OutputTo.WriteLine(ToStringAlternative());
                break;
            case ConsoleTableEnums.Format.Minimal:
                Options.OutputTo.WriteLine(ToMinimalString());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    private static IEnumerable<string> GetColumns<T>()
    {
        return typeof(T).GetProperties().Select(x => x.Name).ToArray();
    }

    private static object GetColumnValue<T>(object target, string column)
    {
        return typeof(T).GetProperty(column)?.GetValue(target, null)??string.Empty;
    }

    private static IEnumerable<Type> GetColumnsType<T>()
    {
        return typeof(T).GetProperties().Select(x => x.PropertyType).ToArray();
    }

    public static void Print(List<dynamic> data, ConsoleTableEnums.Format format = ConsoleTableEnums.Format.Alternative)
    {
        if (data == null || !data.Any())
        {
            return;
        }
        var ct = new ConsoleTable();
        var firstItem = data.First();
        var headers = ((IDictionary<string, object>)firstItem).Keys.ToList();
        ct.AddColumn(headers);
        foreach(var item in data)
        {
            var values = ((IDictionary<string, object>)item).Values.Select(v => v?.ToString() ?? "null").ToArray();
            ct.AddRow(values);
        }
        ct.Configure(o => {
            o.NumberAlignment = ConsoleTableEnums.Alignment.Right;
            o.EnableCount = false;
        }).Write(format);
    }
}

