using MyJournal.Services;
using MyJournal.Models;
using Microsoft.AspNetCore.Components;

namespace MyJournal.Components.Pages
{
    /// <summary>
    /// Entries list page with search, filter, and pagination
    /// Demonstrates: Data binding, Event handling, List rendering, Pagination
    /// </summary>
    public partial class Entries
    {
        private List<JournalEntry> entries = new();
        private bool isLoading = true;
        private int currentPage = 1;
        private int pageSize = 10;
        private int totalPages = 1;
        private int totalEntries = 0;
        private List<string> availableTags = new();

        // Search and filter properties
        private string searchTerm = "";
        private DateTime? filterStartDate;
        private DateTime? filterEndDate;
        private string selectedMood = "";
        private string selectedTag = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadEntriesAsync();
            await LoadTagsAsync();
        }

        private async Task LoadEntriesAsync()
        {
            isLoading = true;

            try
            {
                totalEntries = await JournalService.GetTotalEntriesCountAsync();
                totalPages = (int)Math.Ceiling((double)totalEntries / pageSize);
                entries = await JournalService.GetAllEntriesAsync(currentPage, pageSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading entries: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task LoadTagsAsync()
        {
            var tags = await JournalService.GetAllTagNamesAsync();
            if (tags.Count == 0)
            {
                availableTags = TagDefinitions.GetAllPreBuiltTags();
            }
            else
            {
                // Merge prebuilt + custom
                var merged = new HashSet<string>(TagDefinitions.GetAllPreBuiltTags(), StringComparer.OrdinalIgnoreCase);
                foreach (var t in tags) merged.Add(t);
                availableTags = merged.OrderBy(t => t).ToList();
            }
        }

        private async Task SearchEntries()
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                entries = await JournalService.SearchEntriesAsync(searchTerm);
                totalPages = 1; // Show all results on one page
                currentPage = 1;
            }
            else
            {
                await LoadEntriesAsync();
            }
        }

        private async Task ApplyDateFilter()
        {
            if (filterStartDate.HasValue && filterEndDate.HasValue)
            {
                entries = await JournalService.FilterByDateRangeAsync(
                    filterStartDate.Value, 
                    filterEndDate.Value
                );
                totalPages = 1;
                currentPage = 1;
            }
        }

        private async Task ApplyMoodFilter()
        {
            if (!string.IsNullOrEmpty(selectedMood))
            {
                entries = await JournalService.FilterByMoodAsync(selectedMood);
                totalPages = 1;
                currentPage = 1;
            }
        }

        private async Task ApplyTagFilter()
        {
            if (!string.IsNullOrEmpty(selectedTag))
            {
                entries = await JournalService.FilterByTagAsync(selectedTag);
                totalPages = 1;
                currentPage = 1;
            }
        }

        private async Task ClearFilters()
        {
            searchTerm = "";
            filterStartDate = null;
            filterEndDate = null;
            selectedMood = "";
            selectedTag = "";
            currentPage = 1;
            await LoadEntriesAsync();
        }

        private async Task NextPage()
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                await LoadEntriesAsync();
            }
        }

        private async Task PreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                await LoadEntriesAsync();
            }
        }

        private void ViewEntry(int entryId)
        {
            Navigation.NavigateTo($"/entry/{entryId}");
        }

        private string GetPreview(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            var plain = MarkdownService.ToPlainText(content);
            return plain.Length > 150
                ? plain.Substring(0, 150) + "..."
                : plain;
        }

        private string GetMoodClass(MoodCategory category)
        {
            return category switch
            {
                MoodCategory.Positive => "mood-positive",
                MoodCategory.Neutral => "mood-neutral",
                MoodCategory.Negative => "mood-negative",
                _ => ""
            };
        }

        private string GetMoodEmoji(MoodCategory category)
        {
            return category switch
            {
                MoodCategory.Positive => "😊",
                MoodCategory.Neutral => "😐",
                MoodCategory.Negative => "😔",
                _ => "😐"
            };
        }

        private string GetCategoryLabel(string? category)
        {
            return string.IsNullOrWhiteSpace(category) ? "General" : category;
        }
    }
}
