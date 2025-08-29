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
            _context = context??throw new ArgumentNullException(nameof(context), "Context cannot be null.");
            _logger = logger??throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        }

        public async Task<Users?> GetActiveUserAsync(LoginDto dto)
        {
            try
            {
                return await _context.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.IsActive && !u.IsDelete);
            }
            catch (DbUpdateException ex)
            {
                _logger.LoggWarning("Null argument in GetActiveUserAsync.", ex);
                throw;
            }

            catch (ArgumentNullException ex)
            {
                _logger.LoggWarning("Null argument in GetActiveUserAsync.", ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "Invalid operation during GetActiveUserAsync.",ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "GetActiveUserAsync - unexpected error occurred.",ex);
                throw;
            }
        }

        public async Task<Users?> GetUserAsync(int userid)
        {
            try
            {
                return await _context.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userid);
            }
            catch (ArgumentException ex)
            {
                _logger.LoggWarning("Invalid input in GetUserAsync.",ex);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex,"Invalid EF operation in GetUserAsync.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "GetUserAsync - unexpected failure.");
                throw;
            }

        }
    }
}
