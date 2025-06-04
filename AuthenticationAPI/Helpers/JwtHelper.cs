using AuthenticationAPI.Configurations;
using AuthenticationAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AuthenticationAPI.Helpers
{
    public class JwtHelper:IJwtHelper
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger _logger;

        public JwtHelper(IOptions<JwtSettings> jwtSettings, ILogger<JwtHelper> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(Users User)
        {
            _logger.LogInformation("Started generating Access Token for UserId: {UserId}, Username: {Username}", User.UserId, User.UserName);
            string roleName = User.Role?.RoleId switch
            {
                1 => "Admin",
                2 => "User",
                _ => "Unknown"
            };
            if (roleName == "Unknown") {
                _logger.LogWarning("Role ID for user {UserId} not recognized: {RoleId}", User.UserId, User.Role?.RoleId);
            }
            _logger.LogInformation("User role resolved: {Role}", roleName);
            var authClaims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, User.UserName),
            new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
            new Claim(ClaimTypes.Role, roleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiration),
                claims: authClaims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken= new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("Access Token successfully generated for UserId: {UserId}", User.UserId);
            return accessToken;
        }


        // Generate Refresh Token (longer expiration)
        public string GenerateRefreshToken(Users user)
        {
            _logger.LogInformation("Started generating Refresh Token for UserId: {UserId}, Username: {Username}", user.UserId, user.UserName);
            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
             new Claim("TokenType", "RefreshToken")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiration),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("Refresh Token successfully generated for UserId: {UserId}", user.UserId);
            return refreshToken;
        }
    }

}
