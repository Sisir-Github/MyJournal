using MyJournal.Models;
using MyJournal.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MyJournal.Services
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
                    IsLockEnabled = false,
                    IsPasswordProtected = false,
                    IsPinProtected = false,
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

        public async Task<bool> HasPinAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.IsPinProtected && !string.IsNullOrEmpty(settings.PinHash);
        }

        public async Task<bool> HasAnyLockAsync()
        {
            var settings = await GetSettingsAsync();
            if (!settings.IsLockEnabled)
            {
                return false;
            }

            return settings.IsPinProtected && !string.IsNullOrEmpty(settings.PinHash);
        }

        public async Task<bool> HasAnyCredentialAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.IsPinProtected && !string.IsNullOrEmpty(settings.PinHash);
        }

        public async Task<bool> IsLockEnabledAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.IsLockEnabled;
        }

        public async Task<bool> SetLockEnabledAsync(bool enabled)
        {
            try
            {
                var settings = await GetSettingsAsync();
                if (enabled)
                {
                    var hasPin = settings.IsPinProtected && !string.IsNullOrEmpty(settings.PinHash);
                    if (!hasPin)
                    {
                        return false;
                    }
                }
                settings.IsLockEnabled = enabled;
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

        public async Task<bool> SetPinAsync(string pin)
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.PinHash = HashPassword(pin);
                settings.IsPinProtected = true;
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
                if (string.IsNullOrEmpty(settings.PinHash))
                {
                    settings.IsLockEnabled = false;
                }
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemovePinAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.PinHash = null;
                settings.IsPinProtected = false;
                if (string.IsNullOrEmpty(settings.PasswordHash))
                {
                    settings.IsLockEnabled = false;
                }
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

        public async Task<bool> VerifyPinAsync(string pin)
        {
            var settings = await GetSettingsAsync();

            if (!settings.IsPinProtected || string.IsNullOrEmpty(settings.PinHash))
            {
                return true;
            }

            var hashedInput = HashPassword(pin);
            return hashedInput == settings.PinHash;
        }

        public async Task<bool> VerifyPasswordOrPinAsync(string input)
        {
            var settings = await GetSettingsAsync();
            var hasPassword = settings.IsPasswordProtected && !string.IsNullOrEmpty(settings.PasswordHash);
            var hasPin = settings.IsPinProtected && !string.IsNullOrEmpty(settings.PinHash);

            if (!hasPassword && !hasPin)
            {
                return true;
            }

            var hashedInput = HashPassword(input);
            return (hasPassword && hashedInput == settings.PasswordHash)
                || (hasPin && hashedInput == settings.PinHash);
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
