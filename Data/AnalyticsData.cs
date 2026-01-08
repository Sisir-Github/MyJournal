using JournalApp.Models;

namespace JournalApp.Data
{
    /// <summary>
    /// Data Transfer Object for analytics
    /// Demonstrates Encapsulation
    /// </summary>
    public class AnalyticsData
    {
        // Mood Distribution
        public int PositiveMoodCount { get; set; }
        public int NeutralMoodCount { get; set; }
        public int NegativeMoodCount { get; set; }
        public double PositiveMoodPercentage { get; set; }
        public double NeutralMoodPercentage { get; set; }
        public double NegativeMoodPercentage { get; set; }

        // Most Frequent Mood
        public string? MostFrequentMood { get; set; }
        public int MostFrequentMoodCount { get; set; }

        // Streak Information
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int MissedDays { get; set; }

        // Tag Analytics
        public Dictionary<string, int> TagUsageCount { get; set; } = new();
        public Dictionary<string, double> TagPercentages { get; set; } = new();

        // Word Count Trends
        public double AverageWordCount { get; set; }
        public int TotalWordCount { get; set; }
        public Dictionary<DateTime, int> DailyWordCounts { get; set; } = new();

        // Entry Statistics
        public int TotalEntries { get; set; }
        public DateTime? FirstEntryDate { get; set; }
        public DateTime? LastEntryDate { get; set; }
    }
}
