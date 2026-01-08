using JournalApp.Data;
using JournalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for calculating analytics and insights
    /// Demonstrates business logic abstraction and data processing
    /// </summary>
    public class AnalyticsService
    {
        private readonly JournalDbContext _context;

        public AnalyticsService(JournalDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get comprehensive analytics for a date range
        /// </summary>
        public async Task<AnalyticsData> GetAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Default to all time if no dates specified
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            var entries = await _context.JournalEntries
                .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
                .ToListAsync();

            var analytics = new AnalyticsData
            {
                TotalEntries = entries.Count
            };

            if (entries.Count == 0)
            {
                return analytics;
            }

            analytics.FirstEntryDate = entries.Min(e => e.EntryDate);
            analytics.LastEntryDate = entries.Max(e => e.EntryDate);

            // Calculate mood distribution
            CalculateMoodDistribution(entries, analytics);

            // Calculate most frequent mood
            CalculateMostFrequentMood(entries, analytics);

            // Calculate streaks
            await CalculateStreaksAsync(analytics);

            // Calculate tag analytics
            CalculateTagAnalytics(entries, analytics);

            // Calculate word count trends
            CalculateWordCountTrends(entries, analytics);

            return analytics;
        }

        private void CalculateMoodDistribution(List<JournalEntry> entries, AnalyticsData analytics)
        {
            // Count all moods (primary and secondary)
            var allMoods = new List<(string Mood, MoodCategory Category)>();

            foreach (var entry in entries)
            {
                allMoods.Add((entry.PrimaryMood, entry.PrimaryMoodCategory));
                
                if (!string.IsNullOrEmpty(entry.SecondaryMood1) && entry.SecondaryMood1Category.HasValue)
                {
                    allMoods.Add((entry.SecondaryMood1, entry.SecondaryMood1Category.Value));
                }
                
                if (!string.IsNullOrEmpty(entry.SecondaryMood2) && entry.SecondaryMood2Category.HasValue)
                {
                    allMoods.Add((entry.SecondaryMood2, entry.SecondaryMood2Category.Value));
                }
            }

            analytics.PositiveMoodCount = allMoods.Count(m => m.Category == MoodCategory.Positive);
            analytics.NeutralMoodCount = allMoods.Count(m => m.Category == MoodCategory.Neutral);
            analytics.NegativeMoodCount = allMoods.Count(m => m.Category == MoodCategory.Negative);

            int totalMoods = allMoods.Count;
            if (totalMoods > 0)
            {
                analytics.PositiveMoodPercentage = Math.Round((double)analytics.PositiveMoodCount / totalMoods * 100, 2);
                analytics.NeutralMoodPercentage = Math.Round((double)analytics.NeutralMoodCount / totalMoods * 100, 2);
                analytics.NegativeMoodPercentage = Math.Round((double)analytics.NegativeMoodCount / totalMoods * 100, 2);
            }
        }

        private void CalculateMostFrequentMood(List<JournalEntry> entries, AnalyticsData analytics)
        {
            var moodCounts = new Dictionary<string, int>();

            foreach (var entry in entries)
            {
                IncrementMoodCount(moodCounts, entry.PrimaryMood);
                
                if (!string.IsNullOrEmpty(entry.SecondaryMood1))
                {
                    IncrementMoodCount(moodCounts, entry.SecondaryMood1);
                }
                
                if (!string.IsNullOrEmpty(entry.SecondaryMood2))
                {
                    IncrementMoodCount(moodCounts, entry.SecondaryMood2);
                }
            }

            if (moodCounts.Any())
            {
                var mostFrequent = moodCounts.OrderByDescending(kvp => kvp.Value).First();
                analytics.MostFrequentMood = mostFrequent.Key;
                analytics.MostFrequentMoodCount = mostFrequent.Value;
            }
        }

        private async Task CalculateStreaksAsync(AnalyticsData analytics)
        {
            var allEntries = await _context.JournalEntries
                .OrderBy(e => e.EntryDate)
                .Select(e => e.EntryDate.Date)
                .ToListAsync();

            if (!allEntries.Any())
            {
                return;
            }

            // Calculate current streak
            analytics.CurrentStreak = CalculateCurrentStreak(allEntries);

            // Calculate longest streak
            analytics.LongestStreak = CalculateLongestStreak(allEntries);

            // Calculate missed days
            var firstDate = allEntries.First();
            var lastDate = allEntries.Last();
            var totalDays = (lastDate - firstDate).Days + 1;
            analytics.MissedDays = totalDays - allEntries.Distinct().Count();
        }

        private int CalculateCurrentStreak(List<DateTime> entryDates)
        {
            var today = DateTime.Today;
            var currentStreak = 0;

            // Check if there's an entry for today or yesterday
            var lastEntryDate = entryDates.LastOrDefault();
            if (lastEntryDate < today.AddDays(-1))
            {
                return 0; // Streak is broken
            }

            // Count backwards from today
            var checkDate = today;
            while (entryDates.Contains(checkDate) || 
                   (checkDate == today && entryDates.Contains(checkDate.AddDays(-1))))
            {
                if (entryDates.Contains(checkDate))
                {
                    currentStreak++;
                }
                checkDate = checkDate.AddDays(-1);
            }

            return currentStreak;
        }

        private int CalculateLongestStreak(List<DateTime> entryDates)
        {
            if (!entryDates.Any())
            {
                return 0;
            }

            var longestStreak = 1;
            var currentStreakCount = 1;
            var distinctDates = entryDates.Distinct().OrderBy(d => d).ToList();

            for (int i = 1; i < distinctDates.Count; i++)
            {
                if ((distinctDates[i] - distinctDates[i - 1]).Days == 1)
                {
                    currentStreakCount++;
                    longestStreak = Math.Max(longestStreak, currentStreakCount);
                }
                else
                {
                    currentStreakCount = 1;
                }
            }

            return longestStreak;
        }

        private void CalculateTagAnalytics(List<JournalEntry> entries, AnalyticsData analytics)
        {
            var tagCounts = new Dictionary<string, int>();

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Tags))
                {
                    var tags = entry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                    {
                        var trimmedTag = tag.Trim();
                        if (!tagCounts.ContainsKey(trimmedTag))
                        {
                            tagCounts[trimmedTag] = 0;
                        }
                        tagCounts[trimmedTag]++;
                    }
                }
            }

            analytics.TagUsageCount = tagCounts.OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Calculate tag percentages
            var totalTags = tagCounts.Values.Sum();
            if (totalTags > 0)
            {
                analytics.TagPercentages = tagCounts.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Math.Round((double)kvp.Value / totalTags * 100, 2)
                );
            }
        }

        private void CalculateWordCountTrends(List<JournalEntry> entries, AnalyticsData analytics)
        {
            analytics.TotalWordCount = entries.Sum(e => e.WordCount);
            analytics.AverageWordCount = entries.Any() 
                ? Math.Round(entries.Average(e => e.WordCount), 2) 
                : 0;

            analytics.DailyWordCounts = entries
                .GroupBy(e => e.EntryDate.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.WordCount));
        }

        private void IncrementMoodCount(Dictionary<string, int> moodCounts, string mood)
        {
            if (!string.IsNullOrEmpty(mood))
            {
                if (!moodCounts.ContainsKey(mood))
                {
                    moodCounts[mood] = 0;
                }
                moodCounts[mood]++;
            }
        }
    }
}
