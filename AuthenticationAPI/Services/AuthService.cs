using AuthenticationAPI.Configurations;
using AuthenticationAPI.Data;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthenticationAPI.Services
{
    public class AuthService:IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtHelper _jwtHelper;

        public AuthService(ApplicationDbContext db,IOptions<JwtSettings> jwtSettings)
        {
            _db = db;
            _jwtHelper=new JwtHelper(jwtSettings);

        }
        public async Task<Object>LoginAsync(LoginDTO dto)
        {
            var user=await _db.User.Include(u=>u.Role).FirstOrDefaultAsync(u=>u.UserName==dto.UserName && u.IsActive && !u.IsDelete);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid Username or Password.");


            // Generate tokens
            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken(user);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(60);  // Set refresh token expiry time (can be longer)
            await _db.SaveChangesAsync();

            return new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken  // Send back only the access token ideally
            };
        }
    }
}
