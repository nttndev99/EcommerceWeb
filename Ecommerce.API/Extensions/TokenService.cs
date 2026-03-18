using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ecommerce.Domain.Entities;

namespace Ecommerce.API.Extensions;

// ─────────────────────────────────────────────
// JWT SETTINGS
// ─────────────────────────────────────────────
public class JwtSettings
{
    public string SecretKey      { get; set; } = string.Empty;
    public string Issuer         { get; set; } = string.Empty;
    public string Audience       { get; set; } = string.Empty;
    public int    ExpiryMinutes  { get; set; } = 60;
    public int    RefreshExpDays { get; set; } = 7;
}

// ─────────────────────────────────────────────
// TOKEN SERVICE
// ─────────────────────────────────────────────
public interface ITokenService
{
    Task<string> GenerateTokenAsync(AppUser user);
}

public class TokenService : ITokenService
{
    private readonly JwtSettings         _jwt;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(JwtSettings jwt, UserManager<AppUser> userManager)
    {
        _jwt         = jwt;
        _userManager = userManager;
    }

    public async Task<string> GenerateTokenAsync(AppUser user)
    {
        var roles  = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("fullName",                    user.FullName),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _jwt.Issuer,
            audience:           _jwt.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}