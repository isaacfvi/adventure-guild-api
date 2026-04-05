using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class JwtAuthExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("Variável de ambiente JWT_SECRET não definida.");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new InvalidOperationException("Variável de ambiente JWT_ISSUER não definida.");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new InvalidOperationException("Variável de ambiente JWT_AUDIENCE não definida.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var message = context.AuthenticateFailure is not null
                            ? "Token inválido ou expirado"
                            : "Token de autenticação não fornecido";

                        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? string.Empty;

                        var errorResponse = new ErrorResponse
                        {
                            StatusCode = 401,
                            Message = message,
                            CorrelationId = correlationId
                        };

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var json = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                        await context.Response.WriteAsync(json);
                    },

                    OnForbidden = async context =>
                    {
                        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? string.Empty;

                        var errorResponse = new ErrorResponse
                        {
                            StatusCode = 403,
                            Message = "Acesso negado: permissão insuficiente",
                            CorrelationId = correlationId
                        };

                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var json = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                        await context.Response.WriteAsync(json);
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
