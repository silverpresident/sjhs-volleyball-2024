using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Configuration;
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
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Register custom claims principal factory to load roles into claims
            // This ensures user roles are available during authentication for all login methods
            services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>,
                CustomUserClaimsPrincipalFactory>();
 
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
                options.Events.OnCreatingTicket = async context =>
                {
                    // Extract email from Google profile
                    var email = context.Principal?.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (string.IsNullOrEmpty(email))
                    {
                        return;
                    }

                    // Get services from DI container
                    var services = context.HttpContext.RequestServices;
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("GoogleOAuth");

                    try
                    {
                        // Find the user in the database
                        var user = await userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            // User will be created in ExternalLoginCallback, skip role assignment here
                            return;
                        }

                        // Get role configuration
                        var adminEmails = configuration.GetSection("VolleyBallRallyManager:DefaultAdminEmails").Get<string[]>();
                        var judgeEmails = configuration.GetSection("VolleyBallRallyManager:DefaultJudgeEmails").Get<string[]>();
                        var scorekeeperEmails = configuration.GetSection("VolleyBallRallyManager:DefaultScorekeeperEmails").Get<string[]>();

                        // Assign roles to database (not just claims)
                        // These will be loaded into claims by CustomUserClaimsPrincipalFactory
                        if (adminEmails?.Contains(email, StringComparer.InvariantCultureIgnoreCase) == true)
                        {
                            if (!await userManager.IsInRoleAsync(user, "Administrator"))
                            {
                                var result = await userManager.AddToRoleAsync(user, "Administrator");
                                if (result.Succeeded)
                                {
                                    logger.LogInformation("Assigned Administrator role to user {Email} during OAuth", email);
                                }
                            }
                        }
                        else if (judgeEmails?.Contains(email, StringComparer.InvariantCultureIgnoreCase) == true)
                        {
                            if (!await userManager.IsInRoleAsync(user, "Judge"))
                            {
                                var result = await userManager.AddToRoleAsync(user, "Judge");
                                if (result.Succeeded)
                                {
                                    logger.LogInformation("Assigned Judge role to user {Email} during OAuth", email);
                                }
                            }
                        }
                        else if (scorekeeperEmails?.Contains(email, StringComparer.InvariantCultureIgnoreCase) == true)
                        {
                            if (!await userManager.IsInRoleAsync(user, "Scorekeeper"))
                            {
                                var result = await userManager.AddToRoleAsync(user, "Scorekeeper");
                                if (result.Succeeded)
                                {
                                    logger.LogInformation("Assigned Scorekeeper role to user {Email} during OAuth", email);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error assigning roles during Google OAuth for user {Email}", email);
                    }
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
