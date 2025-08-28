using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using NPOI.SS.Formula.Eval;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Repositories;

namespace TaskManagementWebAPI.Application.Services
{
    public class ForgotPasswordHandler : IForgotPasswordHandler
    {
        private readonly IAppLogger<ForgotPasswordHandler> _logger;
        private readonly IUserApplicationService _userApplicationService;
        public ForgotPasswordHandler(IUserRepository userRepo, IEmailService emailService, 
            IAppLogger<ForgotPasswordHandler> logger, IUserApplicationService userApplicationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService));
        }

        public async Task<Users?> HandleAsync(ForgotPasswordRequest request)
        {
            try
            { 
                var user = await _userApplicationService.ForgotPassword(request.Email);
                // if (user is null) 
                return user; // No email leaks 
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
            catch (Exception ex)
            {
                _logger.LoggError(ex, "ForgotPassword - Unexpected error occurred");
                throw;
            }
        }
    }
}
