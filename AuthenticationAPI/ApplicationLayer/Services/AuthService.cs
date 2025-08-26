using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.InfrastructureLayer.Data;
using AuthenticationAPI.InfrastructureLayer.Helpers;
using AuthenticationAPI.Repositories;
using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationAPI.Services
{
    public class AuthService : IAuthService
    { 
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _config;
        private readonly IAppLogger<AuthService> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthService(IConfiguration config, IJwtHelper jwthelper, IAppLogger<AuthService> logger, IAuthRepository authRepository)
        { 
            _jwtHelper = jwthelper ?? throw new ArgumentNullException(nameof(jwthelper));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));

        }
        public async Task<Object?> LoginAsync(LoginDTO dto)
        { 
                var user = await _authRepository.GetActiveUserAsync(dto);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    _logger.LoggWarning("Faild to login attempt for Username: {Username}", dto.UserName); 
                    return null;
                }

                // Generate tokens
                var accessToken = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken(user);
                return new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken   
                }; 
            
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        { 
            var tokenValidationParameters = new TokenValidationParameters
            {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)),
                    ValidateLifetime = false  
             };       

                var tokenHandler = new JwtSecurityTokenHandler(); 
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LoggWarning("Invalid token algorithm");
                    throw new SecurityTokenException("Invalid token");
                }
                return principal;    
             
        }
    }
}
