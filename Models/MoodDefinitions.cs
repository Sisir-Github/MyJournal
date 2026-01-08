namespace JournalApp.Models
{
    /// <summary>
    /// Static class containing all mood definitions
    /// Demonstrates Encapsulation and Abstraction
    /// </summary>
    public static class MoodDefinitions
    {
        public static readonly Dictionary<string, MoodCategory> Moods = new()
        {
            // Positive Moods
            { "Happy", MoodCategory.Positive },
            { "Excited", MoodCategory.Positive },
            { "Relaxed", MoodCategory.Positive },
            { "Grateful", MoodCategory.Positive },
            { "Confident", MoodCategory.Positive },

            // Neutral Moods
            { "Calm", MoodCategory.Neutral },
            { "Thoughtful", MoodCategory.Neutral },
            { "Curious", MoodCategory.Neutral },
            { "Nostalgic", MoodCategory.Neutral },
            { "Bored", MoodCategory.Neutral },

            // Negative Moods
            { "Sad", MoodCategory.Negative },
            { "Angry", MoodCategory.Negative },
            { "Stressed", MoodCategory.Negative },
            { "Lonely", MoodCategory.Negative },
            { "Anxious", MoodCategory.Negative }
        };

        public static List<string> GetMoodsByCategory(MoodCategory category)
        {
            return Moods.Where(m => m.Value == category)
                        .Select(m => m.Key)
                        .ToList();
        }

        public static List<string> GetAllMoods()
        {
            return Moods.Keys.ToList();
        }

        public static MoodCategory GetMoodCategory(string mood)
        {
            return Moods.TryGetValue(mood, out var category) ? category : MoodCategory.Neutral;
        }
    }
}
