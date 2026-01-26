using System.ComponentModel.DataAnnotations;
using MyJournal.Services;
using MyJournal.Models;
using Microsoft.Maui.Storage;
using System.IO;
using Microsoft.AspNetCore.Components;

namespace MyJournal.Components.Pages
{
    /// <summary>
    /// Settings page for theme, security, and export
    /// Demonstrates form handling and settings management
    /// </summary>
    public partial class Settings
    {
        // ✅ Inject here (NOT in .razor)
        [Inject] private SettingsService SettingsService { get; set; } = default!;
        [Inject] private ExportService ExportService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private AppSettings? settings;
        private PinModel pinModel = new();
        private string message = "";
        private bool isSuccess = false;
        private int entriesPerPage = 10;
        private string exportPath = "";
        private bool lockEnabled = false;

        // Export settings
        private DateTime exportStartDate = DateTime.Today.AddMonths(-1);
        private DateTime exportEndDate = DateTime.Today;

        protected override async Task OnInitializedAsync()
        {
            settings = await SettingsService.GetSettingsAsync();
            entriesPerPage = settings.EntriesPerPage;
            lockEnabled = settings.IsLockEnabled;
        }

        private async Task ChangeTheme(string theme)
        {
            var success = await SettingsService.UpdateThemeAsync(theme);
            if (success)
            {
                settings = await SettingsService.GetSettingsAsync();
                message = "Theme updated successfully!";
                isSuccess = true;
            }
            else
            {
                message = "Failed to update theme.";
                isSuccess = false;
            }
        }

        private async Task SetPin()
        {
            if (pinModel.NewPin != pinModel.ConfirmPin)
            {
                message = "PINs do not match.";
                isSuccess = false;
                return;
            }

            if (string.IsNullOrEmpty(pinModel.NewPin) || pinModel.NewPin.Length < 4 || pinModel.NewPin.Length > 6)
            {
                message = "PIN must be 4 to 6 digits.";
                isSuccess = false;
                return;
            }

            if (!pinModel.NewPin.All(char.IsDigit))
            {
                message = "PIN must contain only numbers.";
                isSuccess = false;
                return;
            }

            var success = await SettingsService.SetPinAsync(pinModel.NewPin);
            if (success)
            {
                settings = await SettingsService.GetSettingsAsync();
                message = "PIN protection enabled successfully!";
                isSuccess = true;
                pinModel = new PinModel();
            }
            else
            {
                message = "Failed to set PIN.";
                isSuccess = false;
            }
        }

        private async Task ChangePin()
        {
            if (string.IsNullOrEmpty(pinModel.CurrentPin))
            {
                message = "Please enter your current PIN.";
                isSuccess = false;
                return;
            }

            var isValid = await SettingsService.VerifyPinAsync(pinModel.CurrentPin);
            if (!isValid)
            {
                message = "Current PIN is incorrect.";
                isSuccess = false;
                return;
            }

            if (pinModel.NewPin != pinModel.ConfirmPin)
            {
                message = "New PINs do not match.";
                isSuccess = false;
                return;
            }

            if (string.IsNullOrEmpty(pinModel.NewPin) || pinModel.NewPin.Length < 4 || pinModel.NewPin.Length > 6)
            {
                message = "PIN must be 4 to 6 digits.";
                isSuccess = false;
                return;
            }

            if (!pinModel.NewPin.All(char.IsDigit))
            {
                message = "PIN must contain only numbers.";
                isSuccess = false;
                return;
            }

            var success = await SettingsService.SetPinAsync(pinModel.NewPin);
            if (success)
            {
                message = "PIN changed successfully!";
                isSuccess = true;
                pinModel = new PinModel();
            }
            else
            {
                message = "Failed to change PIN.";
                isSuccess = false;
            }
        }

        private async Task RemovePin()
        {
            var success = await SettingsService.RemovePinAsync();
            if (success)
            {
                settings = await SettingsService.GetSettingsAsync();
                lockEnabled = settings.IsLockEnabled;
                message = "PIN protection removed successfully!";
                isSuccess = true;
            }
            else
            {
                message = "Failed to remove PIN protection.";
                isSuccess = false;
            }
        }

        private async Task UpdatePagination()
        {
            var success = await SettingsService.UpdateEntriesPerPageAsync(entriesPerPage);
            if (success)
            {
                message = "Pagination settings updated successfully!";
                isSuccess = true;
            }
            else
            {
                message = "Failed to update pagination settings.";
                isSuccess = false;
            }
        }

        private async Task ExportToPdf()
        {
            if (exportEndDate < exportStartDate)
            {
                message = "End date must be after start date.";
                isSuccess = false;
                exportPath = "";
                return;
            }

            var exportDir = Path.Combine(FileSystem.Current.AppDataDirectory, "Exports");
            var fileName = $"journal-export-{exportStartDate:yyyyMMdd}-{exportEndDate:yyyyMMdd}.pdf";
            var filePath = Path.Combine(exportDir, fileName);

            var result = await ExportService.ExportToPdfAsync(exportStartDate, exportEndDate, filePath);
            message = result;
            isSuccess = result.StartsWith("Successfully", StringComparison.OrdinalIgnoreCase);
            exportPath = isSuccess ? filePath : "";
        }

        private async Task UpdateLock()
        {
            if (lockEnabled)
            {
                var hasPin = await SettingsService.HasPinAsync();
                if (!hasPin)
                {
                    message = "Set a PIN before enabling lock.";
                    isSuccess = false;
                    lockEnabled = false;
                    return;
                }
            }

            var success = await SettingsService.SetLockEnabledAsync(lockEnabled);
            if (success)
            {
                settings = await SettingsService.GetSettingsAsync();
                message = lockEnabled ? "Journal lock enabled." : "Journal lock disabled.";
                isSuccess = true;
            }
            else
            {
                message = "Failed to update lock setting.";
                isSuccess = false;
            }
        }
    }

    /// <summary>
    /// Model for password form - demonstrates data annotations
    /// </summary>
    public class PinModel
    {
        public string CurrentPin { get; set; } = "";

        [Required(ErrorMessage = "PIN is required")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be 4 to 6 digits")]
        public string NewPin { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your PIN")]
        [Compare(nameof(NewPin), ErrorMessage = "PINs do not match")]
        public string ConfirmPin { get; set; } = "";
    }
}
