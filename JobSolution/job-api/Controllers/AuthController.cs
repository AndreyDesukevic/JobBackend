using JobMonitor.Application.Models;
using JobMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace job_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly HeadHunterAuthService _authService;
    private readonly JwtConfig _jwtConfig;
    private readonly HeadHunterConfig _headHunterConfig;

    public AuthController(HeadHunterAuthService authService, IOptions<JwtConfig> jwtConfig, IOptions<HeadHunterConfig> headHunterConfig)
    {
        _authService = authService;
        _jwtConfig = jwtConfig.Value;
        _headHunterConfig = headHunterConfig.Value; // Получаем конфиг HeadHunter
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var authUrl = $"{_headHunterConfig.AuthUrl}?response_type=code&client_id={_headHunterConfig.ClientId}&redirect_uri={_headHunterConfig.RedirectUri}";
        return Redirect(authUrl);
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var accessToken = await _authService.GetAccessTokenAsync(code);
        if (accessToken == null) return Unauthorized();

        var user = await _authService.GetUserAsync(accessToken);
        if (user == null) return Unauthorized();

        var token = GenerateJwtToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true, Secure = true });

        return Ok(new { message = "Authenticated", token });
    }

    private string GenerateJwtToken(HeadHunterUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", $"{user.FirstName} {user.LastName}")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}