using JournalApp.Services;
using JournalApp.Models;
using Microsoft.AspNetCore.Components;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Create/Edit Entry page
    /// Demonstrates: EditForm, Data Validation, Two-way Data Binding, Event Handling
    /// Following all PDF concepts from Lecture 9
    /// </summary>
    public partial class CreateEditEntry
    {
        [Parameter]
        public int? EntryId { get; set; }

        private JournalEntry entry = new()
        {
            EntryDate = DateTime.Today,
            PrimaryMood = "",
            PrimaryMoodCategory = MoodCategory.Neutral
        };

        private string secondaryMood1Temp = "";
        private string secondaryMood2Temp = "";
        private string message = "";
        private bool isSuccess = false;
        private bool IsEditMode => EntryId.HasValue;

        protected override async Task OnInitializedAsync()
        {
            if (IsEditMode && EntryId.HasValue)
            {
                // Load existing entry for editing
                var existingEntry = await JournalService.GetEntryByIdAsync(EntryId.Value);
                if (existingEntry != null)
                {
                    entry = existingEntry;
                    secondaryMood1Temp = entry.SecondaryMood1 ?? "";
                    secondaryMood2Temp = entry.SecondaryMood2 ?? "";
                }
                else
                {
                    message = "Entry not found.";
                    isSuccess = false;
                }
            }
            else
            {
                // Check if entry already exists for today
                var todayEntry = await JournalService.GetEntryByDateAsync(DateTime.Today);
                if (todayEntry != null)
                {
                    message = "An entry already exists for today. Redirecting to edit...";
                    await Task.Delay(1500);
                    Navigation.NavigateTo($"/edit-entry/{todayEntry.Id}");
                }
            }
        }

        // Event handler for primary mood change
        private void OnPrimaryMoodChanged(ChangeEventArgs e)
        {
            var selectedMood = e.Value?.ToString();
            if (!string.IsNullOrEmpty(selectedMood))
            {
                entry.PrimaryMood = selectedMood;
                entry.PrimaryMoodCategory = MoodDefinitions.GetMoodCategory(selectedMood);
            }
        }

        // Event handler for secondary mood 1 change
        private void OnSecondaryMood1Changed(ChangeEventArgs e)
        {
            var selectedMood = e.Value?.ToString();
            if (!string.IsNullOrEmpty(selectedMood))
            {
                entry.SecondaryMood1 = selectedMood;
                entry.SecondaryMood1Category = MoodDefinitions.GetMoodCategory(selectedMood);
            }
            else
            {
                entry.SecondaryMood1 = null;
                entry.SecondaryMood1Category = null;
            }
        }

        // Event handler for secondary mood 2 change
        private void OnSecondaryMood2Changed(ChangeEventArgs e)
        {
            var selectedMood = e.Value?.ToString();
            if (!string.IsNullOrEmpty(selectedMood))
            {
                entry.SecondaryMood2 = selectedMood;
                entry.SecondaryMood2Category = MoodDefinitions.GetMoodCategory(selectedMood);
            }
            else
            {
                entry.SecondaryMood2 = null;
                entry.SecondaryMood2Category = null;
            }
        }

        // Handle valid form submission
        private async Task HandleValidSubmit()
        {
            try
            {
                bool success;
                if (IsEditMode)
                {
                    success = await JournalService.UpdateEntryAsync(entry);
                    message = success ? "Entry updated successfully!" : "Failed to update entry.";
                }
                else
                {
                    success = await JournalService.CreateEntryAsync(entry);
                    message = success ? "Entry created successfully!" : "An entry already exists for this date.";
                }

                isSuccess = success;

                if (success)
                {
                    await Task.Delay(1500);
                    Navigation.NavigateTo("/entries");
                }
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
                isSuccess = false;
            }
        }

        // Handle invalid form submission
        private void HandleInvalidSubmit()
        {
            message = "Please correct the errors in the form.";
            isSuccess = false;
        }

        // Calculate word count - helper method
        private int CalculateWordCount(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        // Add tag to entry
        private void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(entry.Tags))
            {
                entry.Tags = tag;
            }
            else if (!entry.Tags.Contains(tag))
            {
                entry.Tags += $", {tag}";
            }
        }

        // Delete entry
        private async Task DeleteEntry()
        {
            if (IsEditMode && EntryId.HasValue)
            {
                var confirmed = true; // In real app, use confirmation dialog
                if (confirmed)
                {
                    var success = await JournalService.DeleteEntryAsync(EntryId.Value);
                    if (success)
                    {
                        message = "Entry deleted successfully!";
                        isSuccess = true;
                        await Task.Delay(1000);
                        Navigation.NavigateTo("/entries");
                    }
                    else
                    {
                        message = "Failed to delete entry.";
                        isSuccess = false;
                    }
                }
            }
        }

        // Cancel and navigate back
        private void Cancel()
        {
            Navigation.NavigateTo("/entries");
        }
    }
}
