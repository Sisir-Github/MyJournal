using JournalApp.Services;
using JournalApp.Data;
using JournalApp.Models;
using Microsoft.AspNetCore.Components;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Dashboard page - demonstrates data binding and component lifecycle
    /// Following PDF concepts: One-way data binding, Event handling
    /// </summary>
    public partial class Dashboard
    {
        private AnalyticsData? analytics;
        private List<JournalEntry> recentEntries = new();
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            isLoading = true;
            
            try
            {
                // Get analytics for all time
                analytics = await AnalyticsService.GetAnalyticsAsync();

                // Get recent entries (last 5)
                recentEntries = await JournalService.GetAllEntriesAsync(page: 1, pageSize: 5);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private void ViewEntry(int entryId)
        {
            // Navigate to entry detail page - demonstrates Navigation from PDF
            Navigation.NavigateTo($"/entry/{entryId}");
        }
    }
}
