using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VolleyballRallyManager.Lib.Data;

namespace VolleyballRallyManager.Lib.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthorizationBuilder AddVolleyballRallyAuthorization(this IServiceCollection services)
        {
            return services.AddAuthorizationBuilder()
                .AddPolicy("AdminArea", policy => policy.RequireAuthenticatedUser())
                .AddPolicy("RequireAdministrator", policy => policy.RequireRole("Administrator"))
                .AddPolicy("RequireJudge", policy => policy.RequireRole("Administrator", "Judge"))
                .AddPolicy("RequireScorekeeper", policy => policy.RequireRole("Administrator", "Judge", "Scorekeeper"))
                .AddPolicy("RequireAnnouncer", policy => policy.RequireRole("Administrator", "Announcer"));
        }

        public static IServiceCollection AddVolleyBallRallyAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure Entity Framework Identity
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("VolleyballRallyManager.App")
                )
            );

            // Add Identity services
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // Disabled for development
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure authentication
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

            services.AddAuthentication()
            .AddGoogle(options =>
            {
                var googleAuth = configuration.GetSection("Authentication:Google");
                options.ClientId = googleAuth["ClientId"] ??
                    throw new InvalidOperationException("Google ClientId not configured");
                options.ClientSecret = googleAuth["ClientSecret"] ??
                    throw new InvalidOperationException("Google ClientSecret not configured");

                options.SaveTokens = true;
                options.Events.OnCreatingTicket = context =>
                {
                    // Extract additional claims from Google profile if needed
                    var email = context.Principal?.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        // Add role claims based on configuration
                        var adminEmail = configuration["VolleyBallRallyManager:AdminEmail"];
                        var judgeEmails = configuration.GetSection("VolleyBallRallyManager:DefaultJudgeEmails").Get<string[]>();
                        var scorekeeperEmails = configuration.GetSection("VolleyBallRallyManager:DefaultScorekeeperEmails").Get<string[]>();

                        if (email == adminEmail)
                        {
                            context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Role, "Administrator"));
                        }
                        else if (judgeEmails?.Contains(email) == true)
                        {
                            context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Role, "Judge"));
                        }
                        else if (scorekeeperEmails?.Contains(email) == true)
                        {
                            context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Role, "Scorekeeper"));
                        }
                    }

                    return Task.CompletedTask;
                };
            })/*
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? "";
                    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? "";
                });*/;

            // Configure authorization
            services.AddVolleyballRallyAuthorization();

            return services;
        }
    }
}
