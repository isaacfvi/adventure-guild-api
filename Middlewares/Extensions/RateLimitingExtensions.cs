using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        var maxRequests = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_MAX_REQUESTS"), out var max) ? max : 100;
        var windowSeconds = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_WINDOW_SECONDS"), out var window) ? window : 60;

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = maxRequests;
                limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.RejectionStatusCode = 429;

            options.OnRejected = async (context, token) =>
            {
                var retryAfter = windowSeconds;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterTimeSpan))
                    retryAfter = (int)retryAfterTimeSpan.TotalSeconds;

                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();

                var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

                var response = new ErrorResponse
                {
                    StatusCode = 429,
                    Message = $"Limite de requisições excedido. Tente novamente em {retryAfter} segundos",
                    CorrelationId = correlationId
                };

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    token
                );
            };
        });

        return services;
    }
}
