using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a tag that can be applied to journal entries
    /// Demonstrates Encapsulation
    /// </summary>
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        public bool IsPreBuilt { get; set; } = false;

        // Navigation property
        public virtual ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
    }

    /// <summary>
    /// Junction table for many-to-many relationship between Entries and Tags
    /// </summary>
    public class EntryTag
    {
        public int JournalEntryId { get; set; }
        public virtual JournalEntry JournalEntry { get; set; } = null!;

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; } = null!;
    }
}
