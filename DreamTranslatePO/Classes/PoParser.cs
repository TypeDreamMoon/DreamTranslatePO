using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html

namespace DreamTranslatePO.Classes
{
    namespace PoParser
    {
        /// <summary>
        /// PO FILE HEADER
        /// </summary>            
        public class PoHeader
        {
            public string? ProjectIdVersion;
            public DateTime? PotCreationDate;
            public DateTime? PoRevisionDate;
            public string? LanguageTeam;
            public string? Language;
            public string? MimeVersion;
            public string? ContentType;
            public string? ContentTransferEncoding;
            public string? PluralForms;
        }

        /// <summary>
        /// PO FILE ENTRY
        /// </summary>
        public class PoEntry : INotifyPropertyChanged
        {
            public List<string> Key
            {
                get;
                set;
            } = new List<string>(); // #. 注释

            public List<string> SourceLocations
            {
                get;
                set;
            } = new List<string>();

            public List<string> Source
            {
                get;
                set;
            } = new List<string>(); // #: 来源定位

            private List<string>? _comments;

            public List<string> Comments
            {
                get => _comments;
                set
                {
                    _comments = value;
                    OnPropertyChanged();
                }
            }

            private string _msgCtxt = "";
            private string _msgId = "";
            private string _msgStr = "";

            public string MsgCtxt
            {
                get => _msgCtxt;
                set
                {
                    _msgCtxt = value;
                    OnPropertyChanged();
                }
            }

            public string MsgId
            {
                get => _msgId;
                set
                {
                    _msgId = value;
                    OnPropertyChanged();
                }
            }

            public string MsgStr
            {
                get => _msgStr;
                set
                {
                    if (_msgStr != value)
                    {
                        _msgStr = value;
                        OnPropertyChanged(nameof(MsgStr));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(
                [CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Po Tools
        /// </summary>
        public class PoTools
        {
            public static string RemoveBlankLines(string input)
            {
                // 使用正则表达式去除多余的空白字符（包括换行、制表符等），只保留单词间的一个空格
                string result = Regex.Replace(input, @"\s+", " "); // 将多个空白字符替换成单个空格
                result = result.Trim(); // 去掉文本开头和结尾的空格

                return result;
            }
        }

        /// <summary>
        /// PO FILE
        /// </summary>
        public class PoFile
        {
            public PoFile()
            {
            }

            public PoFile(PoFile inFile)
            {
                Header = inFile.Header;
                Entries = inFile.Entries;
                Comments = inFile.Comments;
            }

            public List<string> Comments
            {
                get;
                set;
            } = new List<string>();

            public PoHeader Header
            {
                get;
                set;
            }

            public List<PoEntry> Entries
            {
                get;
                set;
            } = new List<PoEntry>();

            public bool IsValid()
            {
                return Entries.Count > 0;
            }

            public async Task<PoFile> OpenFile(StorageFile inFile)
            {
                var content = await FileIO.ReadTextAsync(inFile);
                PoFile file = OpenFile(PoFileReader.ParseContent(content));

                Header = file.Header;
                Entries = file.Entries;
                Comments = file.Comments;

                return this;
            }

            public PoFile OpenFile(PoFile inFile)
            {
                Header = inFile.Header;
                Entries = inFile.Entries;
                Comments = inFile.Comments;

                return this;
            }

            public string BuildPoFileInformationString()
            {
                var sb = new StringBuilder();

                foreach (var elem in Comments)
                {
                    sb.AppendLine($"{elem}");
                }

                sb.AppendLine($"Project-Id-Version: {Header.ProjectIdVersion}");
                sb.AppendLine($"POT-Creation-Date: {Header.PotCreationDate}");
                sb.AppendLine($"PO-Revision-Date: {Header.PoRevisionDate}");
                sb.AppendLine($"Language-Team: {Header.LanguageTeam}");
                sb.AppendLine($"Language: {Header.Language}");
                sb.AppendLine($"MIME-Version: {Header.MimeVersion}");
                sb.AppendLine($"Content-Type: {Header.ContentType}");
                sb.AppendLine($"Content-Transfer-Encoding: {Header.ContentTransferEncoding}");
                sb.AppendLine($"Plural-Forms: {Header.PluralForms}");

                return sb.ToString();
            }

            public async Task<StorageFile> ExportFile()
            {
                var picker = new FileSavePicker();
                InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
                picker.FileTypeChoices.Add("PO Files", new List<string> { ".po" });
                picker.SuggestedFileName = "translated.po";

                var file = await picker.PickSaveFileAsync();
                if (file == null) return null;

                try
                {
                    var poContent = BuildPoContent(this);
                    await FileIO.WriteTextAsync(file, poContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    Console.WriteLine($"成功导出至: {file.Path}");
                    return file;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"导出失败: {ex.Message}");
                    return null;
                }
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
            
            private static string ArrayToString(List<string> strList)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var elem in strList)
                {
                    if (elem.Length != 0)
                    {
                        sb.AppendLine(elem);                        
                    }
                }

                return sb.ToString();
            }

            public static string BuildPoContent(PoFile InpoFile)
            {
                var sb = new StringBuilder();

                // Build Heander

                // Comments
                foreach (var elem in InpoFile.Comments)
                {
                    sb.AppendLine($"# {elem}");
                }

                // Header
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

                // Build Entries
                foreach (var entry in InpoFile.Entries)
                {
                    // Comments

                    // Entry Key
                    sb.Append($"#. Key: {ArrayToString(entry.Key)}");
                    // Entry Source
                    sb.Append($"#. SourceLocation: {ArrayToString(entry.SourceLocations)}");
                    // Entry Source
                    sb.Append($"#: Source: {ArrayToString(entry.Source)}");

                    // Content

                    // MsgCtxt
                    if (!string.IsNullOrEmpty(entry.MsgCtxt))
                    {
                        sb.AppendLine($"msgctxt {FormatPoString(entry.MsgCtxt)}");
                    }

                    // MsgId
                    sb.AppendLine($"msgid {FormatPoString(entry.MsgId)}");
                    // MsgStr
                    sb.AppendLine($"msgstr {FormatPoString(entry.MsgStr)}");

                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// PO FILE PARSER
        /// </summary>
        public static class PoFileReader
        {
            public static PoFile ParseContent(string content)
            {
                var lines = content.Replace("\r\n", "\n").Split('\n');
                return ParseLines(lines);
            }

            private static PoFile ParseLines(string[] lines)
            {
                var poFile = new PoFile();
                int i = 0;

                // 收集开头的文件注释
                while (i < lines.Length && lines[i].StartsWith("#") && !lines[i].StartsWith("#.") && !lines[i].StartsWith("#:"))
                {
                    poFile.Comments.Add(lines[i].TrimStart('#').Trim());
                    i++;
                }

                // 跳过空行
                while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;
                // header
                if (i < lines.Length && lines[i].StartsWith("msgid \"\""))
                {
                    i++;
                    while (i < lines.Length && !lines[i].StartsWith("msgstr \"\"")) i++;
                    if (i < lines.Length && lines[i].StartsWith("msgstr \"\""))
                    {
                        i++;
                        var headerLines = new List<string>();
                        while (i < lines.Length && lines[i].StartsWith("\""))
                        {
                            headerLines.Add(lines[i].Trim('"'));
                            i++;
                        }

                        poFile.Header = ParseHeader(headerLines);
                    }
                }

                // 解析每个条目
                while (i < lines.Length)
                {
                    // 跳过空行
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        i++;
                        continue;
                    }

                    // 初始化
                    var entry = new PoEntry();

                    // 收集条目注释
                    while (i < lines.Length && (lines[i].StartsWith("#.") || lines[i].StartsWith("#:")))
                    {
                        var line = lines[i].Trim();

                        if (line.StartsWith("#. Key:"))
                            entry.Key.Add(line.Substring(7).Trim());
                        else if (line.StartsWith("#. SourceLocation:"))
                            entry.SourceLocations.Add(line.Substring(18).Trim());
                        else if (line.StartsWith("#:"))
                            entry.Source.Add(line.Substring(2).Trim());

                        i++;
                    }

                    // msgctxt (可选)
                    if (i < lines.Length && lines[i].StartsWith("msgctxt "))
                    {
                        entry.MsgCtxt = TrimQuotes(lines[i].Substring(8));
                        i++;
                    }

                    // msgid
                    if (i < lines.Length && lines[i].StartsWith("msgid "))
                    {
                        entry.MsgId = ReadMultiline(lines, ref i, "msgid");
                    }

                    // msgstr
                    if (i < lines.Length && lines[i].StartsWith("msgstr "))
                    {
                        entry.MsgStr = ReadMultiline(lines, ref i, "msgstr");
                    }

                    poFile.Entries.Add(entry);

                    // 跳过可能的空行
                    while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;
                }

                return poFile;
            }

            private static PoHeader ParseHeader(List<string> lines)
            {
                var h = new PoHeader();
                foreach (var l in lines)
                {
                    var parts = l.Split(new[] { ':' }, 2);
                    if (parts.Length != 2) continue;
                    var key = parts[0].Trim();
                    var val = parts[1].Trim().Replace("\\n", "");
                    switch (key)
                    {
                        case "Project-Id-Version": h.ProjectIdVersion = val; break;
                        case "POT-Creation-Date":
                            try
                            {
                                h.PotCreationDate = TryParseDateTime(val);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }

                            break;

                        case "PO-Revision-Date":
                            try
                            {
                                h.PoRevisionDate = TryParseDateTime(val);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }

                            break;
                        case "Language-Team": h.LanguageTeam = val; break;
                        case "Language": h.Language = val; break;
                        case "MIME-Version": h.MimeVersion = val; break;
                        case "Content-Type": h.ContentType = val; break;
                        case "Content-Transfer-Encoding": h.ContentTransferEncoding = val; break;
                        case "Plural-Forms": h.PluralForms = val; break;
                    }
                }

                return h;
            }

            private static DateTime? TryParseDateTime(string dateTimeStr)
            {
                // 定义多个日期格式，兼容不同的情况
                string[] formats = new string[]
                {
                    "yyyy-MM-dd HH:mm", // 没有时区
                    "yyyy-MM-dd HH:mmzzz", // 带时区（+0800）
                    "yyyy-MM-dd HH:mmK", // 另一种时区格式（如：2025-04-28 17:29+08:00）
                    "yyyy-MM-dd HH:mm:ss", // 带秒的格式
                    "yyyy-MM-dd HH:mm:sszzz", // 带秒和时区的格式
                    "yyyy-MM-dd HH:mm:ssK" // 带秒和时区格式（如：2025-04-28 17:29:30+08:00）
                };

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(dateTimeStr, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }

                // 如果所有格式都不匹配，返回null
                return null;
            }

            private static string ReadMultiline(string[] lines, ref int idx, string tag)
            {
                var text = TrimQuotes(lines[idx].Substring(tag.Length).Trim());
                idx++;
                while (idx < lines.Length && lines[idx].StartsWith("\""))
                {
                    text += TrimQuotes(lines[idx].Trim());
                    idx++;
                }

                return text;
            }

            private static string TrimQuotes(string s) => s.Trim('"');
        }
    }
}