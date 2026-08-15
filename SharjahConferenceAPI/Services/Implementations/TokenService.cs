namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharjahConferenceAPI.Services.Interfaces;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public (string Token, DateTime Expiration) GenerateToken(User user)
    {
        var secretKey = configuration["Jwt:SecretKey"] ?? "SharjahConferenceDefaultSecretKeyForJwtAuthentication2026!#*";
        var issuer = configuration["Jwt:Issuer"] ?? "SharjahConferenceAPI";
        var audience = configuration["Jwt:Audience"] ?? "SharjahConferenceClients";
        var expirationMinutes = int.TryParse(configuration["Jwt:ExpirationInMinutes"], out var minutes) ? minutes : 1440;

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.PublisherId.HasValue)
        {
            claims.Add(new Claim("PublisherId", user.PublisherId.Value.ToString()));
        }

        if (user.ParticipantId.HasValue)
        {
            claims.Add(new Claim("ParticipantId", user.ParticipantId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiration);
    }
}
