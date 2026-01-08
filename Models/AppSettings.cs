using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models
{
    /// <summary>
    /// Stores application settings like theme and security
    /// Demonstrates Encapsulation with validation
    /// </summary>
    public class AppSettings
    {
        [Key]
        public int Id { get; set; }

        // Security
        public bool IsPasswordProtected { get; set; } = false;

        [StringLength(500)]
        public string? PasswordHash { get; set; }

        // Theme Settings
        [Required]
        [StringLength(20)]
        public string Theme { get; set; } = "Light";

        // User Preferences
        public int EntriesPerPage { get; set; } = 10;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
