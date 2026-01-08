using JournalApp.Models;
using JournalApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for managing application settings including security
    /// Demonstrates encapsulation and security best practices
    /// </summary>
    public class SettingsService
    {
        private readonly JournalDbContext _context;

        public SettingsService(JournalDbContext context)
        {
            _context = context;
        }

        public event Action<string>? ThemeChanged;

        /// <summary>
        /// Get application settings
        /// </summary>
        public async Task<AppSettings> GetSettingsAsync()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSettings
                {
                    Theme = "Light",
                    IsPasswordProtected = false,
                    EntriesPerPage = 10
                };
                _context.AppSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        /// <summary>
        /// ✅ NEW: Check if password protection is enabled
        /// Used by lock screen and App.razor to decide if app should start locked
        /// </summary>
        public async Task<bool> HasPasswordAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.IsPasswordProtected && !string.IsNullOrEmpty(settings.PasswordHash);
        }

        /// <summary>
        /// Update theme setting
        /// </summary>
        public async Task<bool> UpdateThemeAsync(string theme)
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.Theme = theme;
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                ThemeChanged?.Invoke(settings.Theme);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Set password protection
        /// </summary>
        public async Task<bool> SetPasswordAsync(string password)
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.PasswordHash = HashPassword(password);
                settings.IsPasswordProtected = true;
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove password protection
        /// </summary>
        public async Task<bool> RemovePasswordAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.PasswordHash = null;
                settings.IsPasswordProtected = false;
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verify password
        /// </summary>
        public async Task<bool> VerifyPasswordAsync(string password)
        {
            var settings = await GetSettingsAsync();

            if (!settings.IsPasswordProtected || string.IsNullOrEmpty(settings.PasswordHash))
            {
                return true;
            }

            var hashedInput = HashPassword(password);
            return hashedInput == settings.PasswordHash;
        }

        /// <summary>
        /// Update entries per page setting
        /// </summary>
        public async Task<bool> UpdateEntriesPerPageAsync(int count)
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.EntriesPerPage = count;
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
