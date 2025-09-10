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
            _jwtSettings = jwtSettings.Value??throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger??throw new ArgumentNullException(nameof(logger));
        }

        public string GenerateAccessToken(Users User)
        {
            try
            { 
                string roleName = User.Role?.RoleId switch
                {
                    1 => "Admin",
                    2 => "User",
                    _ => "Unknown"
                };
                if (roleName == "Unknown")
                {
                    _logger.LoggWarning($"Unknown role for user {User.UserName} (ID: {User.UserId})");
                    throw new UnauthorizedAccessException("User role is not recognized.");
                } 
                var authClaims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, User.UserName),
            new Claim(ClaimTypes.NameIdentifier, User.UserId.ToString()),
            new Claim(ClaimTypes.Role, roleName)
            };
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    _logger.LoggWarning("JWT_SECRET_KEY environment variable is missing.");
                    throw new SecurityTokenException("JWT secret key not configured.");
                }
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));//Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiration),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                var accessToken = new JwtSecurityTokenHandler().WriteToken(token); 
                return accessToken;
            }
            catch (Exception ex) {
                _logger.LoggError(ex,"GenerateAccessToken failed");
                throw;
            }
           
        }


        // Generate Refresh Token (longer expiration)
        public string GenerateRefreshToken(Users user)
        {
            try
            { 
                var claims = new List<Claim>
            {
             new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
             new Claim("TokenType", "RefreshToken")
            };
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    _logger.LoggWarning("JWT_SECRET_KEY environment variable is missing.");
                    throw new SecurityTokenException("JWT secret key not configured.");
                }
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
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
                _logger.LoggError(ex,"GenerateRefreshToken failed");
                throw;
            }
        }
    }

}
