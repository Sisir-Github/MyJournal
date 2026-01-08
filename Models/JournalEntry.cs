using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a single journal entry with mood tracking and tagging
    /// Demonstrates Encapsulation with private fields and public properties
    /// </summary>
    public class JournalEntry
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 10000 characters")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateTime EntryDate { get; set; }

        // System-generated timestamps (Encapsulation - cannot be set directly)
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Mood Tracking - Primary Mood (Required)
        [Required(ErrorMessage = "Primary mood is required")]
        public string PrimaryMood { get; set; } = string.Empty;

        [Required(ErrorMessage = "Primary mood category is required")]
        public MoodCategory PrimaryMoodCategory { get; set; }

        // Secondary Moods (Optional - up to 2)
        public string? SecondaryMood1 { get; set; }
        public MoodCategory? SecondaryMood1Category { get; set; }

        public string? SecondaryMood2 { get; set; }
        public MoodCategory? SecondaryMood2Category { get; set; }

        // Tags - stored as comma-separated values
        public string? Tags { get; set; }

        // Word count for analytics
        public int WordCount { get; set; }

        // Navigation property for related tags
        public virtual ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
    }

    /// <summary>
    /// Mood categories for classification
    /// </summary>
    public enum MoodCategory
    {
        Positive,
        Neutral,
        Negative
    }
}
