using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolleyballRallyManager.Lib.Hubs;
using VolleyballRallyManager.Lib.Services;

namespace VolleyballRallyManager.Lib.Configuration;

public static class SignalRConfiguration
{
    public static IServiceCollection AddVolleyballSignalR(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100 KB
            options.StreamBufferCapacity = 10;
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

        return services;
    }

    public static IApplicationBuilder UseVolleyballSignalR(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<MatchHub>("/volleyballhub", options =>
            {
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                   Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
                
                options.ApplicationMaxBufferSize = 102400; // 100 KB
                options.TransportMaxBufferSize = 102400; // 100 KB
            });
        });

        return app;
    }
}
