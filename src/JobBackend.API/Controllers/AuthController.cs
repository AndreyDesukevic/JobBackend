using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Domain.Interfaces.Services;
using JobBackend.Domain.Models.Configs;
using JobBackend.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly HeadHunterAuthService _authService;
    private readonly JwtConfig _jwtConfig;
    private readonly HeadHunterConfig _headHunterConfig;
    private readonly IUserService _userService;
    private readonly IAppTokenService _appTokenService;
    private readonly IHhTokenService _hhTokenService;

    public AuthController(HeadHunterAuthService authService, 
        IOptions<JwtConfig> jwtConfig, 
        IOptions<HeadHunterConfig> 
        headHunterConfig, 
        IHhTokenService hhTokenService, 
        IUserService userService,
        IAppTokenService appTokenService)
    {
        _authService = authService;
        _jwtConfig = jwtConfig.Value;
        _headHunterConfig = headHunterConfig.Value;
        _hhTokenService = hhTokenService;
        _userService = userService;
        _appTokenService = appTokenService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var authUrl = $"{_headHunterConfig.AuthUrl}?response_type=code&client_id={_headHunterConfig.ClientId}&redirect_uri={_headHunterConfig.RedirectUri}";
        return Redirect(authUrl);
    }
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Authorization code is missing");

        var tokenResponse = await _authService.GetAccessTokenAsync(code);
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            return Unauthorized();

        var hhUser = await _authService.GetUserAsync(tokenResponse.AccessToken);
        if (hhUser == null)
            return Unauthorized();

        // Проверка существующего пользователя
        var existingUser = await _userService.GetByHhIdAsync(hhUser.Id, cancellationToken);

        if (existingUser == null)
        {
            // Новый пользователь
            var newUser = new User
            {
                HhId = hhUser.Id,
                Email = hhUser.Email,
                FirstName = hhUser.FirstName,
                LastName = hhUser.LastName,
                Phone = hhUser.Phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var usaid = await _userService.AddAsync(newUser, cancellationToken);

            existingUser = await _userService.GetByHhIdAsync(hhUser.Id, cancellationToken);
            if (existingUser == null)
                return StatusCode(500, "Failed to create user");

            var hhToken = new HhToken
            {
                UserId = existingUser.Id,
                HhAccessToken = tokenResponse.AccessToken,
                HhRefreshToken = tokenResponse.RefreshToken,
                HhExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _hhTokenService.AddAsync(hhToken, cancellationToken);
        }
        else
        {
            // Обновление токена, если устарел или отличается
            var existingToken = await _hhTokenService.GetByUserIdAsync(existingUser.Id, cancellationToken);
            var newExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            var needUpdate = existingToken == null ||
                             existingToken.HhExpiresAt < DateTime.UtcNow ||
                             existingToken.HhAccessToken != tokenResponse.AccessToken;

            if (needUpdate)
            {
                var updatedToken = new HhToken
                {
                    UserId = existingUser.Id,
                    HhAccessToken = tokenResponse.AccessToken,
                    HhRefreshToken = tokenResponse.RefreshToken,
                    HhExpiresAt = newExpiry,
                    CreatedAt = existingToken?.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (existingToken == null)
                    await _hhTokenService.AddAsync(updatedToken, cancellationToken);
                else
                    await _hhTokenService.UpdateAsync(updatedToken, cancellationToken);
            }
        }

        // Наш токен истекает чуть раньше — на 5 минут
        var appExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 300);
        var jwtToken = GenerateJwtToken(existingUser, appExpiresAt);

        var existingAppToken = await _appTokenService.GetLatestByUserIdAsync(existingUser.Id, cancellationToken);
        var appToken = new AppToken
        {
            UserId = existingUser.Id,
            AccessToken = jwtToken,
            ExpiresAt = appExpiresAt,
            CreatedAt = existingAppToken?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (existingAppToken == null)
            await _appTokenService.AddAsync(appToken, cancellationToken);
        else
            await _appTokenService.UpdateAsync(appToken, cancellationToken);


        return Ok(new
        {
            access_token = jwtToken,
            expires_at = appExpiresAt
        });
    }

    private string GenerateJwtToken(User user, DateTime expiresAt)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.HhId),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("name", $"{user.FirstName} {user.LastName}".Trim()),
        new Claim("phone", user.Phone ?? string.Empty)
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}