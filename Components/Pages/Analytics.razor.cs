using JournalApp.Services;
using JournalApp.Data;
using Microsoft.AspNetCore.Components;

namespace JournalApp.Components.Pages
{
    public partial class Analytics
    {
        [Inject] public AnalyticsService AnalyticsService { get; set; } = default!;

        private AnalyticsData? analytics;
        private bool isLoading = true;
        private DateTime? startDate;
        private DateTime? endDate;

        protected override async Task OnInitializedAsync()
        {
            endDate = DateTime.Today;
            startDate = DateTime.Today.AddDays(-30);

            await LoadAnalyticsAsync();
        }

        private async Task LoadAnalyticsAsync()
        {
            isLoading = true;

            try
            {
                analytics = await AnalyticsService.GetAnalyticsAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading analytics: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
