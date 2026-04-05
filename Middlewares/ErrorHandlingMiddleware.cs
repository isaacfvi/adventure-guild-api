using System.ComponentModel.DataAnnotations;
using System.Text.Json;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IEventBus eventBus) : IAppMiddleware
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception of type {ExceptionType}: {Message}", ex.GetType().Name, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException or ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.")
        };

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            CorrelationId = correlationId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await context.Response.WriteAsync(json);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            try
            {
                eventBus.Publish(new HttpErrorAuditEvent
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    StatusCode = statusCode,
                    ExceptionType = exception.GetType().Name,
                    ExceptionMessage = exception.Message,
                    CorrelationId = correlationId
                }, "http.error.audit");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao publicar evento de erro HTTP.");
            }
        }
    }
}
