namespace VolleyballRallyManager.Lib.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
}
