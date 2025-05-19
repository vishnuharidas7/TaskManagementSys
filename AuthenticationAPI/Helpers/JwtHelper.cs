﻿using AuthenticationAPI.Configurations;
using AuthenticationAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AuthenticationAPI.Helpers
{
    public class JwtHelper
    {
        private readonly JwtSettings _jwtSettings;

        public JwtHelper(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings= jwtSettings.Value;
        }

        public string GenerateAccessToken(Users User)
        {
            var authClaims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, User.UserName),
            new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
            new Claim(ClaimTypes.Role, User.Role?.RoleName ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiration),
                claims: authClaims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Generate Refresh Token (longer expiration)
        public string GenerateRefreshToken(Users user)
        {
            var refreshToken = Guid.NewGuid().ToString();  // Unique value for refresh token
            return refreshToken;
        }
    }

}
