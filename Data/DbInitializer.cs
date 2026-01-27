using System.Data;
using Microsoft.EntityFrameworkCore;
using MyJournal.Models;

namespace MyJournal.Data
{
    public static class DbInitializer
    {
        public static void EnsureSchema(JournalDbContext context)
        {
            EnsureCategoryColumn(context);
            EnsurePinColumns(context);
            EnsureLockColumn(context);
            EnsureSeedEntries(context);
        }

        private static void EnsureCategoryColumn(JournalDbContext context)
        {
            if (ColumnExists(context, "JournalEntries", "Category"))
            {
                return;
            }

            // Add Category column with default value for existing rows
            context.Database.ExecuteSqlRaw("ALTER TABLE JournalEntries ADD COLUMN Category TEXT NOT NULL DEFAULT 'General';");
            context.Database.ExecuteSqlRaw("UPDATE JournalEntries SET Category='General' WHERE Category IS NULL OR Category='';");
        }

        private static bool ColumnExists(JournalDbContext context, string table, string column)
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({table});";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader["name"]?.ToString();
                if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsurePinColumns(JournalDbContext context)
        {
            if (!ColumnExists(context, "AppSettings", "IsPinProtected"))
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE AppSettings ADD COLUMN IsPinProtected INTEGER NOT NULL DEFAULT 0;");
            }

            if (!ColumnExists(context, "AppSettings", "PinHash"))
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE AppSettings ADD COLUMN PinHash TEXT NULL;");
            }
        }

        private static void EnsureLockColumn(JournalDbContext context)
        {
            if (!ColumnExists(context, "AppSettings", "IsLockEnabled"))
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE AppSettings ADD COLUMN IsLockEnabled INTEGER NOT NULL DEFAULT 0;");
            }
        }

        private static void EnsureSeedEntries(JournalDbContext context)
        {
            if (context.JournalEntries.Any())
            {
                return;
            }

            var entries = new List<JournalEntry>
            {
                CreateEntry(
                    title: "Feeling Overwhelmed but Learning",
                    date: new DateTime(2026, 1, 25),
                    category: "Mental Health",
                    content:
@"Today was mentally exhausting. Some features in the application didn't work as expected, and debugging took longer than planned. I felt frustrated at times, but I reminded myself that mistakes are part of learning.

By the end of the day, I fixed the main issue and understood the root cause. Even though progress was slow, the learning was valuable.",
                    primaryMood: "Sad", primaryCat: MoodCategory.Negative,
                    secondary1: "Thoughtful", secondary1Cat: MoodCategory.Neutral,
                    secondary2: "Bored", secondary2Cat: MoodCategory.Neutral,
                    tags: "debugging, stress, learning, mental-health"
                ),
                CreateEntry(
                    title: "Small Wins Matter",
                    date: new DateTime(2026, 1, 26),
                    category: "Career",
                    content:
@"Today was better than yesterday. I completed the analytics dashboard layout and tested mood filtering. Seeing data change visually based on entries felt rewarding.

This reminded me that even small progress adds up. Staying consistent is more important than working perfectly.",
                    primaryMood: "Happy", primaryCat: MoodCategory.Positive,
                    secondary1: "Confident", secondary1Cat: MoodCategory.Positive,
                    secondary2: "Relaxed", secondary2Cat: MoodCategory.Positive,
                    tags: "career, analytics, progress, motivation"
                ),
                CreateEntry(
                    title: "A Quiet and Reflective Day",
                    date: new DateTime(2026, 1, 27),
                    category: "Personal Reflection",
                    content:
@"Today was calm and quiet. I reviewed previous journal entries and noticed patterns in mood and productivity. Writing regularly helped me understand myself better.

I didn't rush tasks today and focused on clarity instead of speed. The day felt peaceful and balanced.",
                    primaryMood: "Calm", primaryCat: MoodCategory.Neutral,
                    secondary1: "Nostalgic", secondary1Cat: MoodCategory.Neutral,
                    secondary2: "Grateful", secondary2Cat: MoodCategory.Positive,
                    tags: "reflection, calm, balance, journaling"
                )
            };

            context.JournalEntries.AddRange(entries);
            context.SaveChanges();
        }

        private static JournalEntry CreateEntry(
            string title,
            DateTime date,
            string category,
            string content,
            string primaryMood,
            MoodCategory primaryCat,
            string? secondary1,
            MoodCategory? secondary1Cat,
            string? secondary2,
            MoodCategory? secondary2Cat,
            string? tags)
        {
            var now = DateTime.Now;
            return new JournalEntry
            {
                Title = title,
                EntryDate = date,
                Category = category,
                Content = content,
                PrimaryMood = primaryMood,
                PrimaryMoodCategory = primaryCat,
                SecondaryMood1 = secondary1,
                SecondaryMood1Category = secondary1Cat,
                SecondaryMood2 = secondary2,
                SecondaryMood2Category = secondary2Cat,
                Tags = tags,
                WordCount = CalculateWordCount(content),
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        private static int CalculateWordCount(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return 0;
            }

            var words = content.Split(new[] { ' ', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }
    }
}
