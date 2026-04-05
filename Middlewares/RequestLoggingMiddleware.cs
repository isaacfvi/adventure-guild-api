using System.Diagnostics;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IEventBus eventBus) : IAppMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var method = context.Request.Method;
        var path = context.Request.Path;
        var query = context.Request.QueryString;

        logger.LogInformation(
            "Incoming request: {Method} {Path}{Query} | CorrelationId: {CorrelationId}",
            method, path, query, correlationId);

        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        var elapsed = stopwatch.ElapsedMilliseconds;
        var statusCode = context.Response.StatusCode;

        logger.LogInformation(
            "Outgoing response: {StatusCode} | Elapsed: {Elapsed}ms | CorrelationId: {CorrelationId}",
            statusCode, elapsed, correlationId);

        if (elapsed > 2000)
            logger.LogWarning(
                "Slow request detected: {Method} {Path} took {Elapsed}ms | CorrelationId: {CorrelationId}",
                method, path, elapsed, correlationId);

        try
        {
            eventBus.Publish(new HttpRequestAuditEvent
            {
                Method = method,
                Path = path,
                StatusCode = statusCode,
                ElapsedMs = elapsed,
                CorrelationId = correlationId
            }, "http.request.audit");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao publicar evento de auditoria HTTP.");
        }
    }
}
