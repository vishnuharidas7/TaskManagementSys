using AuthenticationAPI.Configurations;
using AuthenticationAPI.Data;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationAPI.Services
{
    public class AuthService:IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        public AuthService(ApplicationDbContext db, IConfiguration config,IJwtHelper jwthelper  ,ILogger<AuthService> logger)
        {
            _db = db;
            _jwtHelper= jwthelper;
            _config = config;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Object>LoginAsync(LoginDTO dto)
        {
            _logger.LogInformation("AuthService-LoginAsync called");
            var user=await _db.User.Include(u=>u.Role).FirstOrDefaultAsync(u=>u.UserName==dto.UserName && u.IsActive && !u.IsDelete);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                _logger.LogWarning("Faild to login attempt for Username: {Username}", dto.UserName);
                throw new UnauthorizedAccessException("Invalid Username or Password.");
            }


            _logger.LogInformation("Started generating Token..");
            // Generate tokens
            string accessToken;
            string refreshToken;
            try
            {
                 accessToken = _jwtHelper.GenerateAccessToken(user);
                _logger.LogInformation("Access token generated successfully for User:{Username}",dto.UserName);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to generate access token for user: {Username}", user.UserName);
                throw;
            }
            try
            {
                refreshToken = _jwtHelper.GenerateRefreshToken(user);
                _logger.LogInformation("Refresh token generated successfully for User:{Username}", dto.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate refresh token for user: {Username}", user.UserName);
                throw;
            }
         
            //user.RefreshToken = refreshToken;
            //user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(60);  // Set refresh token expiry time (can be longer)
            //await _db.SaveChangesAsync();
            _logger.LogInformation("Login successful for username: {Username}", user.UserName);

            return new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken  // Send back only the access token ideally
            };
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"])),
                    ValidateLifetime = false // Ignore token expiration
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                _logger.LogInformation("Validating expired token...");
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm");
                    throw new SecurityTokenException("Invalid token");
                }
                _logger.LogInformation("Successfully extracted principal from expired token");
                return principal;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                throw;
            }
            
        }
    }
}
