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

        if (dbSettings == null){
            dbSettings = new DatabaseSettings();
        }
        var connectionString = configuration.GetConnectionString("DefaultDatabase");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database settings are not configured");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
