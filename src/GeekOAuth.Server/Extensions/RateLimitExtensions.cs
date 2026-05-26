using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Server.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddGeekRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                await ValueTask.CompletedTask;
            };

            options.AddPolicy("token-endpoint", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0,
                    }));

            options.AddPolicy("login-page", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0,
                    }));

            options.AddPolicy("authorize-endpoint", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetPartitionKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0,
                    }));
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
