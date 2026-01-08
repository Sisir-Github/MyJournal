using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JournalApp.Data;
using JournalApp.Services;

namespace JournalApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Register Blazor Web View
            builder.Services.AddMauiBlazorWebView();

            // Configure Database Context with SQLite
            builder.Services.AddDbContext<JournalDbContext>(options =>
            {
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "JournalApp",
                    "journal.db"
                );
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                
                options.UseSqlite($"Data Source={dbPath}");
            });

            // Register Services - Dependency Injection (will be covered in future lectures)
            builder.Services.AddScoped<JournalService>();
            builder.Services.AddScoped<AnalyticsService>();
            builder.Services.AddScoped<SettingsService>();
            builder.Services.AddScoped<ExportService>();
            builder.Services.AddSingleton<LockState>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize Database
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
                dbContext.Database.EnsureCreated();
            }

            return app;
        }
    }
}
