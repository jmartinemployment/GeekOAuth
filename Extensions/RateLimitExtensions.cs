using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddGeekRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter("token-endpoint", opt =>
            {
                opt.PermitLimit = 20;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 4;
                opt.QueueLimit = 0;
            });

            options.AddSlidingWindowLimiter("login-page", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 4;
                opt.QueueLimit = 0;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
