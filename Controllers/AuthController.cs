using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private static readonly HashSet<string> _validRoles = ["Guild_Master", "Adventurer"];

    // Gera um token JWT para testes
    // Em produção, a geração de tokens seria responsabilidade de um serviço de identidade dedicado

    // /auth/token
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] GenerateTokenRequest request)
    {
        if (!_validRoles.Contains(request.Role))
            return BadRequest(new ErrorResponse
            {
                StatusCode = 400,
                Message = $"Papel inválido. Use: {string.Join(" ou ", _validRoles)}",
                CorrelationId = HttpContext.Items["CorrelationId"]?.ToString() ?? string.Empty
            });

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, request.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
