using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MyJournal.Services
{
    public static class MarkdownService
    {
        public static string ToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var lines = markdown.Replace("\r\n", "\n").Split('\n');
            var inList = false;

            foreach (var raw in lines)
            {
                var line = raw ?? string.Empty;
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (inList)
                    {
                        sb.Append("</ul>");
                        inList = false;
                    }
                    continue;
                }

                if (IsListItem(line))
                {
                    if (!inList)
                    {
                        sb.Append("<ul>");
                        inList = true;
                    }
                    sb.Append("<li>");
                    sb.Append(FormatInline(RemoveListMarker(line)));
                    sb.Append("</li>");
                    continue;
                }

                if (inList)
                {
                    sb.Append("</ul>");
                    inList = false;
                }

                var headingLevel = GetHeadingLevel(line);
                if (headingLevel > 0)
                {
                    var text = line.Substring(headingLevel).Trim();
                    sb.Append($"<h{headingLevel}>");
                    sb.Append(FormatInline(text));
                    sb.Append($"</h{headingLevel}>");
                }
                else
                {
                    sb.Append("<p>");
                    sb.Append(FormatInline(line));
                    sb.Append("</p>");
                }
            }

            if (inList)
            {
                sb.Append("</ul>");
            }

            return sb.ToString();
        }

        public static string ToPlainText(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            var text = markdown.Replace("\r\n", "\n");
            text = Regex.Replace(text, @"^\s*#{1,6}\s*", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^\s*[-*+]\s+", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "$1 ($2)");
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
            text = Regex.Replace(text, @"\*(.*?)\*", "$1");
            return text.Trim();
        }

        private static bool IsListItem(string line)
        {
            var trimmed = line.TrimStart();
            return trimmed.StartsWith("- ") || trimmed.StartsWith("* ") || trimmed.StartsWith("+ ");
        }

        private static string RemoveListMarker(string line)
        {
            var trimmed = line.TrimStart();
            return trimmed.Length > 2 ? trimmed.Substring(2) : string.Empty;
        }

        private static int GetHeadingLevel(string line)
        {
            var trimmed = line.TrimStart();
            var count = 0;
            while (count < trimmed.Length && trimmed[count] == '#')
            {
                count++;
            }
            if (count == 0 || count > 6)
            {
                return 0;
            }
            if (trimmed.Length > count && trimmed[count] != ' ')
            {
                return 0;
            }
            return count;
        }

        private static string FormatInline(string text)
        {
            var encoded = WebUtility.HtmlEncode(text);

            encoded = Regex.Replace(encoded, @"\[(.+?)\]\((.+?)\)", match =>
            {
                var label = WebUtility.HtmlEncode(match.Groups[1].Value);
                var url = WebUtility.HtmlEncode(match.Groups[2].Value);
                return $"<a href=\"{url}\" target=\"_blank\">{label}</a>";
            });

            encoded = Regex.Replace(encoded, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            encoded = Regex.Replace(encoded, @"\*(?!\*)(.+?)(?<!\*)\*", "<em>$1</em>");

            return encoded;
        }
    }
}
