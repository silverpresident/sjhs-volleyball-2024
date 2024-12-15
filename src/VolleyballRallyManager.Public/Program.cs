using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using VolleyballRallyManager.Public;
using VolleyballRallyManager.Public.Services;
using VolleyballRallyManager.Lib.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for the API
var apiBaseUrl = builder.Configuration["AppSettings:ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Add services
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<AppState>();

// Configure auto-refresh interval
var refreshInterval = builder.Configuration.GetValue<int>("AppSettings:RefreshInterval");
if (refreshInterval > 0)
{
    builder.Services.AddScoped(sp => new RefreshSettings { Interval = refreshInterval });
}

var app = builder.Build();

// Initialize AppState
var appState = app.Services.GetRequiredService<AppState>();
await appState.InitializeAsync();

await app.RunAsync();
