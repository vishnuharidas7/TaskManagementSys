using AuthenticationAPI.ConfigurationLayer;
using AuthenticationAPI.Models;
using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AuthenticationAPI.InfrastructureLayer.Helpers
{
    public class JwtHelper : IJwtHelper
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IAppLogger<JwtHelper> _logger;

        public JwtHelper(IOptions<JwtSettings> jwtSettings, IAppLogger<JwtHelper> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(Users User)
        {
            try
            {
                // _logger.LogInformation("Started generating Access Token for UserId: {UserId}, Username: {Username}", User.UserId, User.UserName);
                string roleName = User.Role?.RoleId switch
                {
                    1 => "Admin",
                    2 => "User",
                    _ => "Unknown"
                };
                if (roleName == "Unknown")
                {
                    // _logger.LogWarning("Role ID for user {UserId} not recognized: {RoleId}", User.UserId, User.Role?.RoleId);
                }
                // _logger.LogInformation("User role resolved: {Role}", roleName);
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

                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                // _logger.LogInformation("Access Token successfully generated for UserId: {UserId}", User.UserId);
                return accessToken;
            }
            catch (Exception ex) {
                _logger.LoggWarning("GenerateAccessToken failed");
                throw;
            }
           
        }


        // Generate Refresh Token (longer expiration)
        public string GenerateRefreshToken(Users user)
        {
            try
            {
                //_logger.LogInformation("Started generating Refresh Token for UserId: {UserId}, Username: {Username}", user.UserId, user.UserName);
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
                _logger.LoggInformation("Refresh Token successfully generated for UserId: {UserId}", user.UserId);
                return refreshToken;
            }
           catch(Exception ex)
            {
                _logger.LoggWarning("GenerateRefreshToken failed");
                throw;
            }
        }
    }

}
