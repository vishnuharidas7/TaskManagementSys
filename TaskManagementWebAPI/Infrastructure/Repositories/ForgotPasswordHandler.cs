using Microsoft.AspNetCore.Identity.Data;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class ForgotPasswordHandler : IForgotPasswordHandler
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;

        public ForgotPasswordHandler(IUserRepository userRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _emailService = emailService;
        }

        public async Task<Users?> HandleAsync(ForgotPasswordRequest request)
        {
            var user = await _userRepo.ForgotPassword(request.Email);
           // if (user is null) 
                return user; // No email leaks



           // await _emailService.SendAsync(user.Email, "Password Reset", );
        }
    }
}
