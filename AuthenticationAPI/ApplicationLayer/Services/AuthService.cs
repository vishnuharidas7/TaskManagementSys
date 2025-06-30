using AuthenticationAPI.InfrastructureLayer.Data;
using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.InfrastructureLayer.Helpers;
using AuthenticationAPI.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoggingLibrary.Interfaces;

namespace AuthenticationAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _config;
        private readonly IAppLogger<AuthService> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthService(ApplicationDbContext db, IConfiguration config, IJwtHelper jwthelper, IAppLogger<AuthService> logger, IAuthRepository authRepository)
        {
            _db = db;
            _jwtHelper = jwthelper;
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authRepository = authRepository;

        }
        public async Task<Object> LoginAsync(LoginDTO dto)
        {
            try
            {
                var user = await _authRepository.GetActiveUserAsync(dto);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    _logger.LoggWarning("Faild to login attempt for Username: {Username}", dto.UserName);
                    //throw new UnauthorizedAccessException("Invalid Username or Password.");
                    return null;
                }

                // Generate tokens
                var accessToken = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken(user);
                return new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken  // Send back only the access token ideally
                };
            }
            catch (Exception ex) {
                _logger.LoggWarning("LoginAsync failed");
                throw;
            }
            
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"])),
                    ValidateLifetime = false // Ignore token expiration
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                //_logger.LogInformation("Validating expired token...");
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LoggWarning("Invalid token algorithm");
                    throw new SecurityTokenException("Invalid token");
                }
                return principal;


            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Error validating refresh token");
                throw;
            }

        }
    }
}
