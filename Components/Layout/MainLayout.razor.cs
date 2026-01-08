using JournalApp.Services;
using Microsoft.AspNetCore.Components;

namespace JournalApp.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject] private SettingsService SettingsService { get; set; } = default!;
        private string themeClass = "theme-light";

        protected override async Task OnInitializedAsync()
        {
            var settings = await SettingsService.GetSettingsAsync();
            ApplyTheme(settings.Theme);
            SettingsService.ThemeChanged += HandleThemeChanged;
        }

        private void HandleThemeChanged(string theme)
        {
            ApplyTheme(theme);
            _ = InvokeAsync(StateHasChanged);
        }

        private void ApplyTheme(string theme)
        {
            themeClass = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase)
                ? "theme-dark"
                : "theme-light";
        }

        public void Dispose()
        {
            SettingsService.ThemeChanged -= HandleThemeChanged;
        }
    }
}
