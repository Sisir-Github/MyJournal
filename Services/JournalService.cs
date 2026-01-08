using JournalApp.Models;
using JournalApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for managing journal entries
    /// Demonstrates business logic layer abstraction and encapsulation
    /// </summary>
    public class JournalService
    {
        private readonly JournalDbContext _context;

        public JournalService(JournalDbContext context)
        {
            _context = context;
        }

        // CREATE: Add a new journal entry (only one per day)
        public async Task<bool> CreateEntryAsync(JournalEntry entry)
        {
            try
            {
                // Check if entry already exists for this date
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(e => e.EntryDate.Date == entry.EntryDate.Date);

                if (existingEntry != null)
                {
                    return false; // Entry already exists for this date
                }

                // Set system timestamps
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;

                // Calculate word count
                entry.WordCount = CalculateWordCount(entry.Content);

                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // READ: Get entry by date
        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(e => e.EntryDate.Date == date.Date);
        }

        // READ: Get entry by ID
        public async Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // READ: Get all entries (paginated)
        public async Task<List<JournalEntry>> GetAllEntriesAsync(int page = 1, int pageSize = 10)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .OrderByDescending(e => e.EntryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // READ: Get total entry count
        public async Task<int> GetTotalEntriesCountAsync()
        {
            return await _context.JournalEntries.CountAsync();
        }

        // UPDATE: Update existing entry
        public async Task<bool> UpdateEntryAsync(JournalEntry entry)
        {
            try
            {
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(e => e.Id == entry.Id);

                if (existingEntry == null)
                {
                    return false;
                }

                // Update fields
                existingEntry.Title = entry.Title;
                existingEntry.Content = entry.Content;
                existingEntry.PrimaryMood = entry.PrimaryMood;
                existingEntry.PrimaryMoodCategory = entry.PrimaryMoodCategory;
                existingEntry.SecondaryMood1 = entry.SecondaryMood1;
                existingEntry.SecondaryMood1Category = entry.SecondaryMood1Category;
                existingEntry.SecondaryMood2 = entry.SecondaryMood2;
                existingEntry.SecondaryMood2Category = entry.SecondaryMood2Category;
                existingEntry.Tags = entry.Tags;
                existingEntry.WordCount = CalculateWordCount(entry.Content);
                existingEntry.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // DELETE: Remove entry
        public async Task<bool> DeleteEntryAsync(int id)
        {
            try
            {
                var entry = await _context.JournalEntries.FindAsync(id);
                if (entry == null)
                {
                    return false;
                }

                _context.JournalEntries.Remove(entry);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // SEARCH: Search entries by title or content
        public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllEntriesAsync();
            }

            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.Title.Contains(searchTerm) || e.Content.Contains(searchTerm))
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        // FILTER: Filter entries by date range
        public async Task<List<JournalEntry>> FilterByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.EntryDate.Date >= startDate.Date && e.EntryDate.Date <= endDate.Date)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        // FILTER: Filter entries by mood
        public async Task<List<JournalEntry>> FilterByMoodAsync(string mood)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.PrimaryMood == mood || 
                           e.SecondaryMood1 == mood || 
                           e.SecondaryMood2 == mood)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        // FILTER: Filter entries by mood category
        public async Task<List<JournalEntry>> FilterByMoodCategoryAsync(MoodCategory category)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.PrimaryMoodCategory == category || 
                           e.SecondaryMood1Category == category || 
                           e.SecondaryMood2Category == category)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        // FILTER: Filter entries by tag
        public async Task<List<JournalEntry>> FilterByTagAsync(string tag)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.Tags != null && e.Tags.Contains(tag))
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        // Helper: Calculate word count
        private int CalculateWordCount(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return 0;
            }

            var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        // Get entries for calendar view (all dates with entries)
        public async Task<List<DateTime>> GetEntryDatesAsync()
        {
            return await _context.JournalEntries
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .ToListAsync();
        }

        // Check if entry exists for a specific date
        public async Task<bool> EntryExistsForDateAsync(DateTime date)
        {
            return await _context.JournalEntries
                .AnyAsync(e => e.EntryDate.Date == date.Date);
        }
    }
}
