using System.Data;
using Microsoft.EntityFrameworkCore;

namespace MyJournal.Data
{
    public static class DbInitializer
    {
        public static void EnsureSchema(JournalDbContext context)
        {
            EnsureCategoryColumn(context);
            EnsurePinColumns(context);
            EnsureLockColumn(context);
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
    }
}
