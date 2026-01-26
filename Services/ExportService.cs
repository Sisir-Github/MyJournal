using MyJournal.Models;
using MyJournal.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using MyJournal.Services;

namespace MyJournal.Services
{
    /// <summary>
    /// Service for exporting journal entries to PDF
    /// Demonstrates file I/O and data transformation
    /// </summary>
    public class ExportService
    {
        private readonly JournalService _journalService;

        public ExportService(JournalService journalService)
        {
            _journalService = journalService;
        }

        /// <summary>
        /// Export entries to PDF by date range
        /// </summary>
        public async Task<string> ExportToPdfAsync(DateTime startDate, DateTime endDate, string filePath)
        {
            try
            {
                if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    filePath += ".pdf";
                }
                var entries = await _journalService.FilterByDateRangeAsync(startDate, endDate);

                if (!entries.Any())
                {
                    return "No entries found for the selected date range.";
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create PDF
                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {
                    // Add title
                    document.Add(new Paragraph("Journal Entries Export")
                        .SetFontSize(24)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER));

                    document.Add(new Paragraph($"Date Range: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                        .SetFontSize(12)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20));

                    // Add each entry
                    foreach (var entry in entries.OrderBy(e => e.EntryDate))
                    {
                        // Entry date and title
                        document.Add(new Paragraph(entry.EntryDate.ToString("MMMM dd, yyyy"))
                            .SetFontSize(14)
                            .SetBold()
                            .SetMarginTop(15));

                        document.Add(new Paragraph(entry.Title)
                            .SetFontSize(16)
                            .SetBold()
                            .SetMarginBottom(5));

                        // Mood information
                        var moodText = $"Mood: {entry.PrimaryMood} ({entry.PrimaryMoodCategory})";
                        if (!string.IsNullOrEmpty(entry.SecondaryMood1))
                        {
                            moodText += $", {entry.SecondaryMood1}";
                        }
                        if (!string.IsNullOrEmpty(entry.SecondaryMood2))
                        {
                            moodText += $", {entry.SecondaryMood2}";
                        }
                        document.Add(new Paragraph(moodText)
                            .SetFontSize(10)
                            .SetItalic()
                            .SetMarginBottom(5));

                        // Tags
                        if (!string.IsNullOrEmpty(entry.Tags))
                        {
                            document.Add(new Paragraph($"Tags: {entry.Tags}")
                                .SetFontSize(10)
                                .SetItalic()
                                .SetMarginBottom(10));
                        }

                        // Category
                        if (!string.IsNullOrEmpty(entry.Category))
                        {
                            document.Add(new Paragraph($"Category: {entry.Category}")
                                .SetFontSize(10)
                                .SetItalic()
                                .SetMarginBottom(5));
                        }

                        // Content
                        document.Add(new Paragraph(MarkdownService.ToPlainText(entry.Content))
                            .SetFontSize(11)
                            .SetMarginBottom(10));

                        // Word count and timestamps
                        document.Add(new Paragraph(
                            $"Word Count: {entry.WordCount} | " +
                            $"Created: {entry.CreatedAt:g} | " +
                            $"Updated: {entry.UpdatedAt:g}")
                            .SetFontSize(8)
                            .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                            .SetMarginBottom(5));

                        // Separator line
                        document.Add(new Paragraph("─".PadRight(80, '─'))
                            .SetMarginBottom(10));
                    }

                    // Add summary at the end
                    document.Add(new Paragraph($"\nTotal Entries: {entries.Count}")
                        .SetFontSize(12)
                        .SetBold()
                        .SetMarginTop(20));
                }

                return $"Successfully exported {entries.Count} entries to {filePath}";
            }
            catch (Exception ex)
            {
                var simpleResult = await ExportToSimplePdfAsync(startDate, endDate, filePath);
                if (simpleResult.StartsWith("Successfully", StringComparison.OrdinalIgnoreCase))
                {
                    return simpleResult;
                }

                var txtPath = Path.ChangeExtension(filePath, ".txt");
                var txtFallback = await ExportToTextAsync(startDate, endDate, txtPath);
                return $"Error exporting to PDF (iText): {ex.GetType().Name}: {ex.Message}. Fallback: {simpleResult}. TXT: {txtFallback}";
            }
        }

        private async Task<string> ExportToSimplePdfAsync(DateTime startDate, DateTime endDate, string filePath)
        {
            try
            {
                var entries = await _journalService.FilterByDateRangeAsync(startDate, endDate);
                if (!entries.Any())
                {
                    return "No entries found for the selected date range.";
                }

                if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    filePath += ".pdf";
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = new List<string>
                {
                    "Journal Entries Export",
                    $"Date Range: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                    ""
                };

                foreach (var entry in entries.OrderBy(e => e.EntryDate))
                {
                    lines.Add($"{entry.EntryDate:MMMM dd, yyyy}");
                    lines.Add(entry.Title);
                    lines.Add($"Mood: {entry.PrimaryMood} ({entry.PrimaryMoodCategory})");
                    if (!string.IsNullOrEmpty(entry.Tags)) lines.Add($"Tags: {entry.Tags}");
                    if (!string.IsNullOrEmpty(entry.Category)) lines.Add($"Category: {entry.Category}");
                    lines.Add(MarkdownService.ToPlainText(entry.Content));
                    lines.Add($"Word Count: {entry.WordCount} | Created: {entry.CreatedAt:g} | Updated: {entry.UpdatedAt:g}");
                    lines.Add(new string('-', 50));
                }

                var wrapped = WrapLines(lines, 90);
                var pages = PaginateLines(wrapped, 40);

                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(fs, System.Text.Encoding.ASCII);

                writer.WriteLine("%PDF-1.4");

                var offsets = new List<long> { 0 };
                long offset = writer.BaseStream.Position;

                int objNum = 1;
                int catalogObj = objNum++;
                int pagesObj = objNum++;
                int fontObj = objNum++;

                var pageObjects = new List<int>();
                var contentObjects = new List<int>();

                foreach (var page in pages)
                {
                    contentObjects.Add(objNum++);
                    pageObjects.Add(objNum++);
                }

                // Font object
                offsets.Add(offset);
                writer.WriteLine($"{fontObj} 0 obj");
                writer.WriteLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
                writer.WriteLine("endobj");
                offset = writer.BaseStream.Position;

                // Content + Page objects
                for (int i = 0; i < pages.Count; i++)
                {
                    var content = BuildPdfContent(pages[i]);

                    offsets.Add(offset);
                    writer.WriteLine($"{contentObjects[i]} 0 obj");
                    writer.WriteLine($"<< /Length {content.Length} >>");
                    writer.WriteLine("stream");
                    writer.Write(content);
                    writer.WriteLine();
                    writer.WriteLine("endstream");
                    writer.WriteLine("endobj");
                    offset = writer.BaseStream.Position;

                    offsets.Add(offset);
                    writer.WriteLine($"{pageObjects[i]} 0 obj");
                    writer.WriteLine("<< /Type /Page");
                    writer.WriteLine($"/Parent {pagesObj} 0 R");
                    writer.WriteLine("/MediaBox [0 0 612 792]");
                    writer.WriteLine($"/Resources << /Font << /F1 {fontObj} 0 R >> >>");
                    writer.WriteLine($"/Contents {contentObjects[i]} 0 R");
                    writer.WriteLine(">>");
                    writer.WriteLine("endobj");
                    offset = writer.BaseStream.Position;
                }

                // Pages object
                offsets.Add(offset);
                writer.WriteLine($"{pagesObj} 0 obj");
                writer.WriteLine($"<< /Type /Pages /Count {pageObjects.Count} /Kids [ {string.Join(" ", pageObjects.Select(p => $"{p} 0 R"))} ] >>");
                writer.WriteLine("endobj");
                offset = writer.BaseStream.Position;

                // Catalog object
                offsets.Add(offset);
                writer.WriteLine($"{catalogObj} 0 obj");
                writer.WriteLine($"<< /Type /Catalog /Pages {pagesObj} 0 R >>");
                writer.WriteLine("endobj");
                offset = writer.BaseStream.Position;

                // XRef
                long xrefStart = offset;
                writer.WriteLine("xref");
                writer.WriteLine($"0 {offsets.Count}");
                writer.WriteLine("0000000000 65535 f ");
                for (int i = 1; i < offsets.Count; i++)
                {
                    writer.WriteLine($"{offsets[i]:0000000000} 00000 n ");
                }

                // Trailer
                writer.WriteLine("trailer");
                writer.WriteLine($"<< /Size {offsets.Count} /Root {catalogObj} 0 R >>");
                writer.WriteLine("startxref");
                writer.WriteLine(xrefStart);
                writer.WriteLine("%%EOF");

                return $"Successfully exported {entries.Count} entries to {filePath}";
            }
            catch (Exception ex)
            {
                return $"Error exporting simple PDF: {ex.GetType().Name}: {ex.Message}";
            }
        }

        private static string BuildPdfContent(List<string> pageLines)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("BT");
            sb.AppendLine("/F1 12 Tf");
            sb.AppendLine("50 760 Td");
            sb.AppendLine("14 TL");
            foreach (var line in pageLines)
            {
                sb.Append("(");
                sb.Append(EscapePdfText(line));
                sb.AppendLine(") Tj");
                sb.AppendLine("T*");
            }
            sb.AppendLine("ET");
            return sb.ToString();
        }

        private static string EscapePdfText(string text)
        {
            var safe = text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            return new string(safe.Select(c => c <= 127 ? c : '?').ToArray());
        }

        private static List<string> WrapLines(IEnumerable<string> lines, int maxLen)
        {
            var wrapped = new List<string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    wrapped.Add(string.Empty);
                    continue;
                }

                var remaining = line.Trim();
                while (remaining.Length > maxLen)
                {
                    var split = remaining.LastIndexOf(' ', maxLen);
                    if (split <= 0) split = maxLen;
                    wrapped.Add(remaining.Substring(0, split).Trim());
                    remaining = remaining.Substring(split).Trim();
                }
                if (remaining.Length > 0) wrapped.Add(remaining);
            }
            return wrapped;
        }

        private static List<List<string>> PaginateLines(List<string> lines, int linesPerPage)
        {
            var pages = new List<List<string>>();
            var current = new List<string>();
            foreach (var line in lines)
            {
                current.Add(line);
                if (current.Count >= linesPerPage)
                {
                    pages.Add(current);
                    current = new List<string>();
                }
            }
            if (current.Count > 0) pages.Add(current);
            return pages;
        }

        private async Task<string> ExportToTextAsync(DateTime startDate, DateTime endDate, string filePath)
        {
            try
            {
                var entries = await _journalService.FilterByDateRangeAsync(startDate, endDate);
                if (!entries.Any())
                {
                    return "No entries found for the selected date range.";
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var writer = new StreamWriter(filePath, false))
                {
                    await writer.WriteLineAsync("Journal Entries Export");
                    await writer.WriteLineAsync($"Date Range: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}");
                    await writer.WriteLineAsync(new string('-', 50));

                    foreach (var entry in entries.OrderBy(e => e.EntryDate))
                    {
                        await writer.WriteLineAsync($"{entry.EntryDate:MMMM dd, yyyy}");
                        await writer.WriteLineAsync(entry.Title);
                        await writer.WriteLineAsync($"Mood: {entry.PrimaryMood} ({entry.PrimaryMoodCategory})");
                        if (!string.IsNullOrEmpty(entry.Tags))
                        {
                            await writer.WriteLineAsync($"Tags: {entry.Tags}");
                        }
                        if (!string.IsNullOrEmpty(entry.Category))
                        {
                            await writer.WriteLineAsync($"Category: {entry.Category}");
                        }
                        await writer.WriteLineAsync(MarkdownService.ToPlainText(entry.Content));
                        await writer.WriteLineAsync($"Word Count: {entry.WordCount} | Created: {entry.CreatedAt:g} | Updated: {entry.UpdatedAt:g}");
                        await writer.WriteLineAsync(new string('-', 50));
                    }
                }

                return $"Successfully exported {entries.Count} entries to {filePath}";
            }
            catch (Exception ex)
            {
                return $"Error exporting to TXT: {ex.GetType().Name}: {ex.Message}";
            }
        }
    }
}
