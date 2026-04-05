public static class RequestLoggingExtensions
{
    public static IServiceCollection AddRequestLogging(this IServiceCollection services)
    {
        return services;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
