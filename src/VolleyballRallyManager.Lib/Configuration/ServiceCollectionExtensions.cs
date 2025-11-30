using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;
using VolleyballRallyManager.Lib.Workers;

namespace VolleyballRallyManager.Lib.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVolleyballRallyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<DatabaseSettings>(
            configuration.GetSection(DatabaseSettings.SectionName));

        // Database
        var dbSettings = configuration
            .GetSection(DatabaseSettings.SectionName)
            .Get<DatabaseSettings>();

        if (dbSettings == null)
        {
            dbSettings = new DatabaseSettings();
        }
            
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database settings are not configured");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Services
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
        services.AddScoped<IBulletinService, BulletinService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<ITournamentService, TournamentService>();
        services.AddScoped<IActiveTournamentService, ActiveTournamentService>();
        services.AddSingleton<GroupService>();

        // Scoring Channel and Workers
        services.AddSingleton<ScoringChannel>();
        services.AddHostedService<ScoringAutomationWorker>();

        // SignalR
        services.AddSignalR();

        return services;
    }
}
