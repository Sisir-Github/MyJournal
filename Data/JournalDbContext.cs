using Microsoft.EntityFrameworkCore;
using JournalApp.Models;

namespace JournalApp.Data
{
    /// <summary>
    /// Database context for Journal Application using SQLite
    /// Demonstrates data layer abstraction and encapsulation
    /// </summary>
    public class JournalDbContext : DbContext
    {
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<EntryTag> EntryTags { get; set; } = null!;
        public DbSet<AppSettings> AppSettings { get; set; } = null!;

        public JournalDbContext(DbContextOptions<JournalDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Get the app data directory
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "JournalApp",
                    "journal.db"
                );

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure EntryTag junction table (many-to-many relationship)
            modelBuilder.Entity<EntryTag>()
                .HasKey(et => new { et.JournalEntryId, et.TagId });

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.JournalEntry)
                .WithMany(e => e.EntryTags)
                .HasForeignKey(et => et.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.Tag)
                .WithMany(t => t.EntryTags)
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure JournalEntry
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => e.EntryDate)
                .IsUnique();

            // Seed pre-built tags
            SeedPreBuiltTags(modelBuilder);

            // Seed default app settings
            modelBuilder.Entity<AppSettings>().HasData(
                new AppSettings
                {
                    Id = 1,
                    Theme = "Light",
                    IsPasswordProtected = false,
                    EntriesPerPage = 10,
                    LastUpdated = DateTime.Now
                }
            );
        }

        private void SeedPreBuiltTags(ModelBuilder modelBuilder)
        {
            var preBuiltTags = TagDefinitions.GetAllPreBuiltTags();
            var tagEntities = new List<Tag>();

            for (int i = 0; i < preBuiltTags.Count; i++)
            {
                tagEntities.Add(new Tag
                {
                    Id = i + 1,
                    Name = preBuiltTags[i],
                    IsPreBuilt = true
                });
            }

            modelBuilder.Entity<Tag>().HasData(tagEntities);
        }
    }
}
