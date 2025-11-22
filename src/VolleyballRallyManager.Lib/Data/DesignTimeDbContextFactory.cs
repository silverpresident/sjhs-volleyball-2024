using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VolleyballRallyManager.Lib.Configuration;

namespace VolleyballRallyManager.Lib.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        var dbSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();

        if (dbSettings == null)
            throw new InvalidOperationException("Database settings are not configured");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(dbSettings.ConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
