using Microsoft.EntityFrameworkCore;
using VolleyballRallyManager.Lib.Configuration;
using VolleyballRallyManager.Lib.Data;
using VolleyballRallyManager.Lib.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add Volleyball Rally Manager services
builder.Services.AddVolleyballRallyServices(builder.Configuration);

// Add SignalR with custom configuration
builder.Services.AddVolleyballSignalR();

// Add Logging
builder.Services.AddLogging();

// Add Identity
builder.Services.AddVolleyBallRallyAuthentication(builder.Configuration);


// Add CORS for Blazor WebAssembly client
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("BlazorPolicy");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
/*
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");
*/
app.MapControllerRoute(
    name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Configure SignalR with custom configuration
app.UseVolleyballSignalR();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        //await context.Database.MigrateAsync();
        await DatabaseInitialization.InitializeDatabaseAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();