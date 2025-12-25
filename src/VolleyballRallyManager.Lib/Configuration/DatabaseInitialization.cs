using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Ensure database is created
            try
            {
            await dbContext.Database.EnsureCreatedAsync();

            // Apply migrations
            await dbContext.Database.MigrateAsync();
 }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
            // Seed initial data if needed
            try
            {
                // Seed default roles and users
                await SeedDefaultRolesAsync(scope.ServiceProvider);
                await SeedDefaultUsersAsync(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
            try
            {
                await SeedTournamentAsync(dbContext);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
            try
            {
                // Seed default chat rooms
                await SeedChatRoomsAsync(dbContext);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding chat rooms.");
            }
            try
            {
                // Seed initial test data only in Development environment
                var environment = config["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development")
                {
                    await SeedInitialDataAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

        }

        private static async Task SeedDefaultRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create default roles if they don't exist
            string[] roles =  { "Administrator", "Judge", "Scorekeeper", "Announcer", "Referee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        private static async Task SeedDefaultUsersAsync(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        
            var defaultUsers = config.GetSection("VolleyBallRallyManager:DefaultUsers").Get<DefaultUser[]>();
            foreach (var defaultUser in defaultUsers)
            {
                if (string.IsNullOrEmpty(defaultUser.Email))
                {
                    continue;
                }
                // Create default user if it doesn't exist
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user != null)
                {
                    if (defaultUser.Role == "Administrator")
                    {
                        if (!await userManager.IsInRoleAsync(user, defaultUser.Role))
                        {
                            await userManager.AddToRoleAsync(user, defaultUser.Role);
                        }
                    }
                    
                    if (defaultUser.ResetPassword && !string.IsNullOrEmpty(defaultUser.Password)){
                        var hasPassword = await userManager.HasPasswordAsync(user);
                        if (hasPassword == false)
                        {
                            await userManager.AddPasswordAsync(user, defaultUser.Password);
                        } else {
                            await userManager.RemovePasswordAsync(user);
                            await userManager.AddPasswordAsync(user, defaultUser.Password);
                        }
                    }
                    continue;
                }
                if (string.IsNullOrEmpty(defaultUser.Password))
                {
                    defaultUser.Password = "sdm-admin2025";
                }
                if (string.IsNullOrEmpty(defaultUser.UserName))
                {
                    var i = defaultUser.Email.IndexOf("@");
                    defaultUser.UserName = defaultUser.Email.Substring(0, i);
                }

                var basicUser = new IdentityUser { UserName = defaultUser.UserName, Email = defaultUser.Email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(basicUser, defaultUser.Password);
                if (result.Succeeded == false)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(defaultUser.Role))
                {
                    defaultUser.Role = "Viewer";
                }
                await userManager.AddToRoleAsync(basicUser, defaultUser.Role);
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

        private static async Task<List<RoundTemplate>> SeedRoundsAsync(ApplicationDbContext dbContext)
        {
            var rounds = new List<RoundTemplate>();
            // Add sample rounds with recommendation properties
            var defaultRounds = new[]
            {
                new RoundTemplate {
                    Id = Guid.NewGuid(),
                    Name = "Preliminary Round",
                    Sequence = 1,
                    RecommendedQualifyingTeamsCount = 0,
                    RecommendedMatchGenerationStrategy = MatchGenerationStrategy.RoundRobin,
                    RecommendedTeamSelectionStrategy = TeamSelectionStrategy.TopFromGroupAndNextBest,
                    IsPlayoff = false
                },
                new RoundTemplate {
                    Id = Guid.NewGuid(),
                    Name = "Seeded Round",
                    Sequence = 2,
                    RecommendedQualifyingTeamsCount = 16,
                    RecommendedMatchGenerationStrategy = MatchGenerationStrategy.GroupStageKnockout,
                    RecommendedTeamSelectionStrategy = TeamSelectionStrategy.TopByPoints,
                    IsPlayoff = false
                },
                new RoundTemplate {
                    Id = Guid.NewGuid(),
                    Name = "Quarter Finals",
                    Sequence = 4,
                    RecommendedQualifyingTeamsCount = 8,
                    RecommendedMatchGenerationStrategy = MatchGenerationStrategy.SeededBracket,
                    RecommendedTeamSelectionStrategy = TeamSelectionStrategy.WinnersOnly,
                    IsPlayoff = true
                },
                new RoundTemplate {
                    Id = Guid.NewGuid(),
                    Name = "Semi Finals",
                    Sequence = 5,
                    RecommendedQualifyingTeamsCount = 4,
                    RecommendedMatchGenerationStrategy = MatchGenerationStrategy.SeededBracket,
                    RecommendedTeamSelectionStrategy = TeamSelectionStrategy.WinnersOnly,
                    IsPlayoff = false
                },
                new RoundTemplate {
                    Id = Guid.NewGuid(),
                    Name = "Third Place Playoff",
                    Sequence = 6,
                    RecommendedQualifyingTeamsCount = 2,
                    RecommendedMatchGenerationStrategy = MatchGenerationStrategy.SeededBracket,
                    RecommendedTeamSelectionStrategy = TeamSelectionStrategy.WinnersOnly,
                    IsPlayoff = true
                },
                new RoundTemplate {
                    Id = Guid.NewGuid(),
                    Name = "Finals",
                    Sequence = 7,
                    RecommendedQualifyingTeamsCount = 2,
                    RecommendedMatchGenerationStrategy = MatchGenerationStrategy.SeededBracket,
                    RecommendedTeamSelectionStrategy = TeamSelectionStrategy.WinnersOnly,
                    IsPlayoff = false
                }
            };
            foreach (var round in defaultRounds)
            {

                if (dbContext.RoundTemplates.Any(d => d.Name == round.Name))
                {
                    rounds.Add(dbContext.RoundTemplates.First(d => d.Name == round.Name));
                }
                else
                {
                    round.UpdatedAt = DateTime.Now;
                    round.CreatedAt = DateTime.Now;
                    round.UpdatedBy = "System";
                    round.CreatedBy = "System";
                    dbContext.RoundTemplates.Add(round);
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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };
                await dbContext.Tournaments.AddAsync(tournament);
            }
            await dbContext.SaveChangesAsync();

        }

        private static async Task SeedChatRoomsAsync(ApplicationDbContext dbContext)
        {
            // Define default system rooms
            var defaultRooms = new[]
            {
                new { Name = "Lobby", Description = "General communication", RoomType = ChatRoomType.Public, RequiredRole = (string?)null },
                new { Name = "Management", Description = "For key organizers", RoomType = ChatRoomType.Private, RequiredRole = (string?)null },
                new { Name = "Judges and Referees", Description = "For official referee communications", RoomType = ChatRoomType.RoleBased, RequiredRole = "Referee" },
                new { Name = "Scorers", Description = "For official score-keeping communications", RoomType = ChatRoomType.RoleBased, RequiredRole = "Scorekeeper" },
                new { Name = "Announcers", Description = "For official announcer communications", RoomType = ChatRoomType.RoleBased, RequiredRole = "Announcer" },
                new { Name = "Support", Description = "For support staff to handle escalated issues", RoomType = ChatRoomType.Private, RequiredRole = (string?)null }
            };

            foreach (var roomDef in defaultRooms)
            {
                // Check if room already exists
                if (!await dbContext.ChatRooms.AnyAsync(r => r.Name == roomDef.Name))
                {
                    var room = new ChatRoom
                    {
                        Id = Guid.NewGuid(),
                        Name = roomDef.Name,
                        Description = roomDef.Description,
                        RoomType = roomDef.RoomType,
                        RequiredRole = roomDef.RequiredRole,
                        IsSystemRoom = true,
                        OwnerId = "System",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    };

                    await dbContext.ChatRooms.AddAsync(room);
                }
            }

            await dbContext.SaveChangesAsync();
        }
        private static async Task SeedInitialDataAsync(ApplicationDbContext dbContext)
        {
            /*if (await dbContext.Teams.AnyAsync())
            {
                return;
            }*/
            // Add sample divisions
            var divisions = await SeedDivisionsAsync(dbContext);
            var rounds = await SeedRoundsAsync(dbContext);


            // Add sample teams
            var teams = new List<Team>();
            /*var teams = new[]
            {
                new Team {
                    TournamentRoundId = Guid.NewGuid(),
                    Name = "Team A",
                    School = "School A",
                    Color = "#FF0000"
                },
                new Team {
                    TournamentRoundId = Guid.NewGuid(),
                    Name = "Team B",
                    School = "School B",
                    Color = "#00FF00"
                },
                new Team {
                    TournamentRoundId = Guid.NewGuid(),
                    Name = "Team C",
                    School = "School C",
                    Color = "#0000FF"
                },
                new Team {
                    TournamentRoundId = Guid.NewGuid(),
                    Name = "Team D",
                    School = "School D",
                    Color = "#FFFF00"
                }
            };*/
            char alphaStart = 'E';
            char alphaEnd = 'X';
            for (char i = alphaStart; i <= alphaEnd; i++)
            {
                string anchorLetter = i.ToString();
                var tn = $"Team {anchorLetter}";
                if (await dbContext.Teams.AnyAsync(t => t.Name == tn))
                {
                    //continue;
                }
                teams.Add(new Team
                {
                    Id = Guid.NewGuid(),
                    Name = tn,
                    School = $"School {anchorLetter}",
                    Color = "#FFFF00"
                });
            }
            foreach (var team in teams)
            {
                team.UpdatedAt = DateTime.Now;
                team.CreatedAt = DateTime.Now;
                team.UpdatedBy = "System";
                team.CreatedBy = "System";
            }
            if (!await dbContext.Teams.AnyAsync())
                await dbContext.Teams.AddRangeAsync(new List<Team>(teams));
            await dbContext.SaveChangesAsync();

            // Add sample bulletin
            var bulletin = new Bulletin
            {
                Id = Guid.NewGuid(),
                Content = "Welcome to ST JAGO VOLLEYBALL RALLY!",
                Priority = BulletinPriority.Info,
                IsVisible = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = "System",
                UpdatedBy = "System"
            };
            if (!await dbContext.Bulletins.AnyAsync())
            {
                await dbContext.Bulletins.AddAsync(bulletin);
            }
            var tournament = await dbContext.Tournaments.FirstOrDefaultAsync();
            if (tournament != null)
            {

                if (!await dbContext.TournamentDivisions.AnyAsync())
                {
                    foreach (var division in divisions)
                    {
                        await dbContext.TournamentDivisions.AddAsync(new TournamentDivision
                        {
                            TournamentId = tournament.Id,
                            Tournament = tournament,
                            DivisionId = division.Id,
                            Division = division
                        });
                    }

                    await dbContext.SaveChangesAsync();

                }

                if (!await dbContext.TournamentTeamDivisions.AnyAsync())
                {
                    var rand = new Random();
                    teams = await dbContext.Teams.ToListAsync();
                    bool sel1 = true;
                    foreach (var team in teams)
                    {
                        sel1 = rand.Next() % 2 == 0;
                        var division = sel1 ? divisions[0] : divisions[1];
                        sel1 = rand.Next() % 2 == 0;

                        await dbContext.TournamentTeamDivisions.AddAsync(new TournamentTeamDivision
                        {
                            TournamentId = tournament.Id,
                            TeamId = team.Id,
                            DivisionId = division.Id,
                            Division = division,
                            GroupName = sel1 ? "A" : "B",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            CreatedBy = "System",
                            UpdatedBy = "System"
                        });
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
    
    internal class DefaultUser
    {
        public string? Email { get; internal set; }
        public string? UserName { get; internal set; }
        public string? Password { get; internal set; }
        public string? Role { get; internal set; }
        public bool ResetPassword { get; internal set; } = false;
    }
}
