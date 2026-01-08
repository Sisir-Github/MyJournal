namespace JournalApp.Models
{
    /// <summary>
    /// Static class containing pre-built tag definitions
    /// Demonstrates Encapsulation and code organization
    /// </summary>
    public static class TagDefinitions
    {
        public static readonly List<string> PreBuiltTags = new()
        {
            // Career & Work
            "Work",
            "Career",
            "Studies",
            "Projects",
            "Planning",

            // Relationships
            "Family",
            "Friends",
            "Relationships",
            "Parenting",

            // Health & Wellness
            "Health",
            "Fitness",
            "Exercise",
            "Meditation",
            "Yoga",
            "Self-care",

            // Personal Development
            "Personal Growth",
            "Reflection",
            "Spirituality",

            // Activities & Hobbies
            "Hobbies",
            "Travel",
            "Nature",
            "Reading",
            "Writing",
            "Cooking",
            "Music",
            "Shopping",

            // Special Occasions
            "Birthday",
            "Holiday",
            "Vacation",
            "Celebration",

            // Other
            "Finance"
        };

        public static List<string> GetAllPreBuiltTags()
        {
            return PreBuiltTags.OrderBy(t => t).ToList();
        }
    }
}
