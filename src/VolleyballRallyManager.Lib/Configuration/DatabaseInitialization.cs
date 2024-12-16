using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;

namespace VolleyballRallyManager.Lib.Configuration
{
    public static class DatabaseInitialization
    {
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply migrations
            await dbContext.Database.MigrateAsync();

            // Seed initial data if needed
            if (!await dbContext.Teams.AnyAsync())
            {
                await SeedInitialDataAsync(dbContext);
            }
        }

        private static async Task SeedInitialDataAsync(ApplicationDbContext dbContext)
        {
            // Add sample rounds
            var rounds = new[]
            {
                new Round { Id = Guid.NewGuid(), Name = "Round 1", Order = 1 },
                new Round { Id = Guid.NewGuid(), Name = "Round 2", Order = 2 },
                new Round { Id = Guid.NewGuid(), Name = "Round 3", Order = 3 },
                new Round { Id = Guid.NewGuid(), Name = "Quarter Finals", Order = 4 },
                new Round { Id = Guid.NewGuid(), Name = "Semi Finals", Order = 5 },
                new Round { Id = Guid.NewGuid(), Name = "Finals", Order = 6 }
            };
            // Add sample divisions
            var divisions = new[]
            {
                new Division { Id = Guid.NewGuid(), Name = "BOYS"},
                new Division { Id = Guid.NewGuid(), Name = "GIRLS"}
            };

            await dbContext.Rounds.AddRangeAsync(rounds);
            await dbContext.SaveChangesAsync();

            // Add sample teams
            var teams = new[]
            {
                new Team { 
                    Id = Guid.NewGuid(), 
                    Name = "Team A", 
                    School = "School A",
                    Color = "#FF0000",
                    Division = divisions[0]
                },
                new Team { 
                    Id = Guid.NewGuid(), 
                    Name = "Team B", 
                    School = "School B",
                    Color = "#00FF00",
                    Division = divisions[0]
                },
                new Team { 
                    Id = Guid.NewGuid(), 
                    Name = "Team C", 
                    School = "School C",
                    Color = "#0000FF",
                    Division = divisions[1]
                },
                new Team { 
                    Id = Guid.NewGuid(), 
                    Name = "Team D", 
                    School = "School D",
                    Color = "#FFFF00",
                    Division = divisions[1]
                }
            };

            await dbContext.Teams.AddRangeAsync(teams);
            await dbContext.SaveChangesAsync();

            // Add sample announcement
            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                Content = "Welcome to ST JAGO VOLLEYBALL RALLY!",
                Priority = AnnouncementPriority.Info,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.Announcements.AddAsync(announcement);
            await dbContext.SaveChangesAsync();
        }
    }
}
