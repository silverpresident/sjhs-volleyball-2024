using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace VolleyballRallyManager.Lib.Configuration
{
    public static class DatabaseInitialization
    {
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            // Apply migrations
            await dbContext.Database.MigrateAsync();

            // Seed initial data if needed
            try
            {
                // Seed default admin user
                await SeedAdminUserAsync(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
            try
            {
                await SeedTournamentAsync(dbContext);

                await SeedInitialDataAsync(dbContext);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

        }

        private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Administrator", "Judge", "Scorekeeper" };
            string AdminRole = "Administrator";
            foreach (string role in roles)
            {
                // Ensure the Admin role exists
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin user if it doesn't exist
            var adminUserDefault = new IdentityUser { UserName = "admin", Email = "admin@example.com" };
            var adminUser = await userManager.FindByNameAsync(adminUserDefault.UserName);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    EmailConfirmed = true,
                    UserName = adminUserDefault.UserName,
                    Email = adminUserDefault.Email
                
                };
                await userManager.CreateAsync(adminUser, "admin123");
            }
            else
            {
                await userManager.SetEmailAsync(adminUser, adminUserDefault.Email);
                // Remove existing password if any, then add the default password
                var hasPassword = await userManager.HasPasswordAsync(adminUser);
                if (hasPassword)
                {
                    await userManager.RemovePasswordAsync(adminUser);
                }
                await userManager.AddPasswordAsync(adminUser, "admin123");
            }
            if (adminUser != null)
            {
                // Assign the Admin role to the user
                if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                }
            }
        }

        private static async Task<List<Division>> SeedDivisionsAsync(ApplicationDbContext dbContext)
        {
            var divisions = new List<Division>();
            // Add sample divisions
            var defaultDivisions = new[]
            {
                new Division {Name = "BOYS"},
                new Division {Name = "GIRLS"}
            };
            foreach (var division in defaultDivisions)
            {
                if (string.IsNullOrEmpty(division.Name))
                {
                    division.Name = "Unknown";
                }
                if (dbContext.Divisions.Any(d => d.Name == division.Name))
                {
                    divisions.Add(dbContext.Divisions.First(d => d.Name == division.Name));
                }
                else
                {
                    division.UpdatedAt = DateTime.Now;
                    division.CreatedAt = DateTime.Now;
                    division.UpdatedBy = "System";
                    division.CreatedBy = "System";
                    dbContext.Divisions.Add(division);
                    divisions.Add(division);
                }
            }
            await dbContext.SaveChangesAsync();
            return divisions;
        }

        private static async Task<List<Round>> SeedRoundsAsync(ApplicationDbContext dbContext)
        {
            var rounds = new List<Round>();
            // Add sample rounds
            var defaultRounds = new[]
            {
                new Round { Id = Guid.NewGuid(), Name = "Round 1", Sequence = 1 },
                new Round { Id = Guid.NewGuid(), Name = "Round 2", Sequence = 2 },
                new Round { Id = Guid.NewGuid(), Name = "Round 3", Sequence = 3 },
                new Round { Id = Guid.NewGuid(), Name = "Quarter Finals", Sequence = 4 },
                new Round { Id = Guid.NewGuid(), Name = "Semi Finals", Sequence = 5 },
                new Round { Id = Guid.NewGuid(), Name = "Finals", Sequence = 6 }
            };
            foreach (var round in defaultRounds)
            {

                if (dbContext.Rounds.Any(d => d.Name == round.Name))
                {
                    rounds.Add(dbContext.Rounds.First(d => d.Name == round.Name));
                }
                else
                {
                    round.UpdatedAt = DateTime.Now;
                    round.CreatedAt = DateTime.Now;
                    round.UpdatedBy = "System";
                    round.CreatedBy = "System";
                    dbContext.Rounds.Add(round);
                    rounds.Add(round);
                }
            }
            await dbContext.SaveChangesAsync();
            return rounds;
        }
        private static async Task SeedTournamentAsync(ApplicationDbContext dbContext)
        {
            string tourName = $"Volleyball {DateTime.Now.Year}";
            if (!await dbContext.Tournaments.AnyAsync())
            {
                var tournament = new Tournament()
                {
                    Description = string.Empty,
                    Name = tourName,
                    IsActive = true,
                    TournamentDate = DateTime.Now.AddHours(9 - DateTime.Now.Hour),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };
                await dbContext.Tournaments.AddAsync(tournament);
            }
            await dbContext.SaveChangesAsync();

        }
        private static async Task SeedInitialDataAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Teams.AnyAsync())
            {
                return;
            }
            // Add sample divisions
            var divisions = await SeedDivisionsAsync(dbContext);
            var rounds = await SeedRoundsAsync(dbContext);


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
            foreach (var team in teams)
            {
                team.UpdatedAt = DateTime.Now;
                team.CreatedAt = DateTime.Now;
                team.UpdatedBy = "System";
                team.CreatedBy = "System";
            }
            if (!await dbContext.Teams.AnyAsync())
                await dbContext.Teams.AddRangeAsync(teams);
            await dbContext.SaveChangesAsync();

            // Add sample announcement
            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                Content = "Welcome to ST JAGO VOLLEYBALL RALLY!",
                Priority = AnnouncementPriority.Info,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };
            if (!await dbContext.Announcements.AnyAsync())
            {
                await dbContext.Announcements.AddAsync(announcement);
            }
            var tournament = await dbContext.Tournaments.FirstOrDefaultAsync();

            if (!await dbContext.TournamentDivisions.AnyAsync())
            {

                if (tournament != null)
                {
                    foreach (var division in divisions)
                    {
                        await dbContext.TournamentDivisions.AddAsync(new TournamentDivision
                        {
                            TournamentId = tournament.Id,
                            DivisionId = division.Id
                        });
                    }
                    await dbContext.SaveChangesAsync();
                }

            }


            if (!await dbContext.TournamentTeamDivisions.AnyAsync())
            {
                teams = await dbContext.Teams.ToArrayAsync();
                bool sel1 = true;
                foreach (var team in teams)
                {
                    var division = sel1 ? divisions[0] : divisions[1];
                    sel1 = !sel1;

                    await dbContext.TournamentTeamDivisions.AddAsync(new TournamentTeamDivision
                    {
                        TournamentId = tournament.Id,
                        TeamId = team.Id,
                        DivisionId = division.Id,
                        GroupName = "A",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    });
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
