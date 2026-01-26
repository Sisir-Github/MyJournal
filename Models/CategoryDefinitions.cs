namespace MyJournal.Models
{
    /// <summary>
    /// Predefined entry categories for organizing journal entries.
    /// </summary>
    public static class CategoryDefinitions
    {
        public static readonly List<string> Categories = new()
        {
            "General",
            "Work",
            "Health",
            "Travel",
            "Fitness",
            "Family",
            "Relationships",
            "Personal Growth",
            "Hobbies",
            "Finance",
            "Spirituality"
        };

        public static List<string> GetAllCategories()
        {
            return Categories.OrderBy(c => c).ToList();
        }
    }
}
