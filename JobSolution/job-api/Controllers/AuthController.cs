using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Services;
using JobMonitor.Domain.Models.Configs;
using JobMonitor.Domain.ResponseModels;
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
    private readonly IUserService _userService;
    private readonly IHhTokenService _hhTokenService;

    public AuthController(HeadHunterAuthService authService, IOptions<JwtConfig> jwtConfig, IOptions<HeadHunterConfig> headHunterConfig, IHhTokenService hhTokenService, IUserService userService)
    {
        _authService = authService;
        _jwtConfig = jwtConfig.Value;
        _headHunterConfig = headHunterConfig.Value;
        _hhTokenService = hhTokenService;
        _userService = userService;
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
        if (string.IsNullOrEmpty(code))
            return BadRequest("Authorization code is missing");

        var tokenResponse = await _authService.GetAccessTokenAsync(code);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            return Unauthorized();

        var hhUser = await _authService.GetUserAsync(tokenResponse.AccessToken);
        if (hhUser == null)
            return Unauthorized();

        // Поиск пользователя по HH ID
        var existingUser = await _userService.GetByHhIdAsync(hhUser.Id, cancellationToken);

        if (existingUser == null)
        {
            // Новый пользователь
            var newUser = new User(
                Id: 0,
                HhId: hhUser.Id,
                Email: hhUser.Email,
                FirstName: hhUser.FirstName,
                LastName: hhUser.LastName,
                Phone: hhUser.Phone,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow
            );

            await _userService.AddAsync(newUser, cancellationToken);

            var addedUser = await _userService.GetByHhIdAsync(hhUser.Id, cancellationToken);
            if (addedUser == null)
                return StatusCode(500, "Failed to create user");

            var hhToken = new HhToken(
                userId: addedUser.Id,
                hhAccessToken: tokenResponse.AccessToken,
                hhRefreshToken: tokenResponse.RefreshToken,
                hhExpiresAt: DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.UtcNow
            );

            await _hhTokenService.AddAsync(hhToken, cancellationToken);
            existingUser = addedUser;
        }
        else
        {
            // Пользователь найден — обновляем токены при необходимости
            var existingToken = await _hhTokenService.GetByUserIdAsync(existingUser.Id, cancellationToken);

            bool needUpdate = existingToken == null ||
                              existingToken.HhExpiresAt < DateTime.UtcNow ||
                              existingToken.HhAccessToken != tokenResponse.AccessToken;

            if (needUpdate)
            {
                var updatedToken = new HhToken(
                    userId: existingUser.Id,
                    hhAccessToken: tokenResponse.AccessToken,
                    hhRefreshToken: tokenResponse.RefreshToken,
                    hhExpiresAt: DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    createdAt: existingToken?.CreatedAt ?? DateTime.UtcNow,
                    updatedAt: DateTime.UtcNow
                );

                if (existingToken == null)
                    await _hhTokenService.AddAsync(updatedToken, cancellationToken);
                else
                    await _hhTokenService.UpdateAsync(updatedToken, cancellationToken);
            }
        }

        // Генерация своего JWT токена, срок жизни на 5 минут меньше HH токена
        var appExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 300);
        var jwtToken = GenerateJwtToken(existingUser, appExpiresAt);

        return Ok(new
        {
            access_token = jwtToken,
            expires_at = appExpiresAt
        });
    }

    private string GenerateJwtToken(HeadHunterUser user)
    {

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("name", $"{user.LastName} {user.FirstName}"),
        new Claim("phone", user.Phone ?? string.Empty),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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