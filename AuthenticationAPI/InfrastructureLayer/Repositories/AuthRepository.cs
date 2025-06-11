using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.InfrastructureLayer.Data;
using AuthenticationAPI.Models;
using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppLogger<AuthRepository> _logger;

        public AuthRepository(ApplicationDbContext context, IAppLogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Users?> GetActiveUserAsync(LoginDTO dto)
        {
            try
            {
                return await _context.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.IsActive && !u.IsDelete);
            }
            catch (Exception ex) {
                _logger.LoggWarning("GetActiveUserAsync-get active user faild");
                throw;
            }
        }

        public async Task<Users?> GetUserAsync(int userid)
        {
            try
            {
                return await _context.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userid);
            }
            catch (Exception ex) {
                _logger.LoggWarning("GetUserAsync-get user faild");
                throw;
            }
           
        }
    }
}
