using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class ForgotPasswordHandler : IForgotPasswordHandler
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly IAppLogger<UserRepository> _logger;
        public ForgotPasswordHandler(IUserRepository userRepo, IEmailService emailService, IAppLogger<UserRepository> logger)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo), "UserRepo cannot be null.");
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService), "EmailService cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Context cannot be null.");
        }

        public async Task<Users?> HandleAsync(ForgotPasswordRequest request)
        {
            try
            {
                var user = await _userRepo.ForgotPassword(request.Email);
                // if (user is null) 
                return user; // No email leaks
                // await _emailService.SendAsync(user.Email, "Password Reset", );
            }

            catch (ArgumentNullException argEx)
            {
                _logger.LoggError(argEx, "ForgotPassword - Null argument: {Message}", argEx.Message);
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LoggError(invOpEx, "ForgotPassword - Invalid operation: {Message}", invOpEx.Message);
                throw;
            }
            //catch (HttpRequestException httpEx)
            //{
            //    _logger.LoggError(httpEx, "ForgotPassword - Network issue occurred: {Message}", httpEx.Message);
            //    throw new HttpRequestException("A network error occurred while processing the forgot password request.", httpEx);
            //}
            catch (Exception ex)
            {
                _logger.LoggError(ex, "ForgotPassword - Unexpected error occurred");
                throw;
            }
        }
    }
}
