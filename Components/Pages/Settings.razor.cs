using System.ComponentModel.DataAnnotations;
using JournalApp.Services;
using JournalApp.Models;
using Microsoft.Maui.Storage;
using System.IO;
using Microsoft.AspNetCore.Components;

namespace JournalApp.Components.Pages
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
        private PasswordModel passwordModel = new();
        private string message = "";
        private bool isSuccess = false;
        private int entriesPerPage = 10;
        private string exportPath = "";

        // Export settings
        private DateTime exportStartDate = DateTime.Today.AddMonths(-1);
        private DateTime exportEndDate = DateTime.Today;

        protected override async Task OnInitializedAsync()
        {
            settings = await SettingsService.GetSettingsAsync();
            entriesPerPage = settings.EntriesPerPage;
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

        private async Task SetPassword()
        {
            if (passwordModel.NewPassword != passwordModel.ConfirmPassword)
            {
                message = "Passwords do not match.";
                isSuccess = false;
                return;
            }

            if (string.IsNullOrEmpty(passwordModel.NewPassword) || passwordModel.NewPassword.Length < 4)
            {
                message = "Password must be at least 4 characters long.";
                isSuccess = false;
                return;
            }

            var success = await SettingsService.SetPasswordAsync(passwordModel.NewPassword);
            if (success)
            {
                settings = await SettingsService.GetSettingsAsync();
                message = "Password protection enabled successfully!";
                isSuccess = true;
                passwordModel = new PasswordModel();
            }
            else
            {
                message = "Failed to set password.";
                isSuccess = false;
            }
        }

        private async Task ChangePassword()
        {
            if (string.IsNullOrEmpty(passwordModel.CurrentPassword))
            {
                message = "Please enter your current password.";
                isSuccess = false;
                return;
            }

            var isValid = await SettingsService.VerifyPasswordAsync(passwordModel.CurrentPassword);
            if (!isValid)
            {
                message = "Current password is incorrect.";
                isSuccess = false;
                return;
            }

            if (passwordModel.NewPassword != passwordModel.ConfirmPassword)
            {
                message = "New passwords do not match.";
                isSuccess = false;
                return;
            }

            if (string.IsNullOrEmpty(passwordModel.NewPassword) || passwordModel.NewPassword.Length < 4)
            {
                message = "Password must be at least 4 characters long.";
                isSuccess = false;
                return;
            }

            var success = await SettingsService.SetPasswordAsync(passwordModel.NewPassword);
            if (success)
            {
                message = "Password changed successfully!";
                isSuccess = true;
                passwordModel = new PasswordModel();
            }
            else
            {
                message = "Failed to change password.";
                isSuccess = false;
            }
        }

        private async Task RemovePassword()
        {
            var success = await SettingsService.RemovePasswordAsync();
            if (success)
            {
                settings = await SettingsService.GetSettingsAsync();
                message = "Password protection removed successfully!";
                isSuccess = true;
            }
            else
            {
                message = "Failed to remove password protection.";
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
    }

    /// <summary>
    /// Model for password form - demonstrates data annotations
    /// </summary>
    public class PasswordModel
    {
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 100 characters")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
