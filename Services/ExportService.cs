using JournalApp.Models;
using JournalApp.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace JournalApp.Services
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

                        // Content
                        document.Add(new Paragraph(entry.Content)
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
                return $"Error exporting to PDF: {ex.Message}";
            }
        }
    }
}
