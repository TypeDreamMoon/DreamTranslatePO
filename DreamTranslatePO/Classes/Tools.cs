using System.Text;
using System.Text.RegularExpressions;
using DreamTranslatePO.Classes.PoParser;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace DreamTranslatePO.Classes;

internal static class Tools
{
    public static string BuildPoContent(PoFile InpoFile)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Modify By DreamTranslatePO.");
        sb.AppendLine("msgid \"\"");
        sb.AppendLine("msgstr \"\"");
        AppendHeaderLine(sb, "Project-Id-Version", InpoFile.Header.ProjectIdVersion + "\\n");
        AppendHeaderLine(sb, "POT-Creation-Date", FormatHeaderDate(InpoFile.Header.PotCreationDate) + "\\n"); // Fmt : 0000-00-00 00:00\n
        AppendHeaderLine(sb, "PO-Revision-Date", FormatHeaderDate(InpoFile.Header.PoRevisionDate) + "\\n"); // Fmt : 0000-00-00 00:00\n
        AppendHeaderLine(sb, "Language-Team", InpoFile.Header.LanguageTeam + "\\n");
        AppendHeaderLine(sb, "Language", InpoFile.Header.Language + "\\n");
        AppendHeaderLine(sb, "MIME-Version", InpoFile.Header.MimeVersion + "\\n");
        AppendHeaderLine(sb, "Content-Type", InpoFile.Header.ContentType + "\\n");
        AppendHeaderLine(sb, "Content-Transfer-Encoding", InpoFile.Header.ContentTransferEncoding + "\\n");
        AppendHeaderLine(sb, "Plural-Forms", InpoFile.Header.PluralForms + "\\n");
        sb.AppendLine();

        foreach (var entry in InpoFile.Entries)
        {
            if (!string.IsNullOrEmpty(entry.MsgCtxt))
            {
                sb.AppendLine($"msgctxt {FormatPoString(entry.MsgCtxt)}");
            }

            sb.AppendLine($"msgid {FormatPoString(entry.MsgId)}");

            sb.AppendLine($"msgstr {FormatPoString(entry.MsgStr)}");

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static void AppendHeaderLine(StringBuilder sb, string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // Format : "key: value"
            sb.AppendLine($"\"{key}: {EscapeHeaderValue(value)}\"");
        }
    }

    public static string FormatHeaderDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
    }

    public static string EscapeHeaderValue(string value)
    {
        return value?.Replace("\"", "\\\"") ?? string.Empty;
    }

    public static string FormatPoString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "\"\"";

        var escaped = new StringBuilder("\"");
        foreach (var c in input)
        {
            switch (c)
            {
                case '\"': escaped.Append("\\\""); break;
                case '\\': escaped.Append("\\\\"); break;
                case '\n': escaped.Append("\\n\"").AppendLine().Append("\""); break;
                default: escaped.Append(c); break;
            }
        }

        escaped.Append('"');

        return escaped.ToString()
            .Replace("\"\n\"", "\"\n\"")
            .TrimEnd('"') + "\"";
    }

    public static string RemoveBlankLines(string input)
    {
        // 使用正则表达式去除多余的空白字符（包括换行、制表符等），只保留单词间的一个空格
        string result = Regex.Replace(input, @"\s+", " ");  // 将多个空白字符替换成单个空格
        result = result.Trim();  // 去掉文本开头和结尾的空格
        
        return result;
    }
}