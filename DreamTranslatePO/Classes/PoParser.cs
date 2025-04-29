using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

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
        /// PO FILE
        /// </summary>
        public class PoFile
        {
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