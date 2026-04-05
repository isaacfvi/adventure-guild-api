using Microsoft.AspNetCore.Mvc;

public static class InputValidationExtensions
{
    public static IServiceCollection AddInputValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err => new ValidationError
                    {
                        Field = e.Key,
                        Message = err.ErrorMessage
                    }))
                    .ToList();

                var response = new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Erro de validação nos dados enviados.",
                    CorrelationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? string.Empty,
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
