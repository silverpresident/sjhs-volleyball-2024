using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Services;
using Microsoft.AspNetCore.Authentication;

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
            throw new InvalidOperationException("Database settings are not configured");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                dbSettings.ConnectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Services
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
        services.AddScoped<TeamService>();
        services.AddScoped<AnnouncementService>();

        // SignalR
        services.AddSignalR();

        return services;
    }
}
