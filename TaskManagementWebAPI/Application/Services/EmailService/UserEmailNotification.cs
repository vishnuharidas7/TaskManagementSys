using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class UserEmailNotification : IUserNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IUserCreatedEmailContentBuilder _userCreatedEmailContentBuilder;
        public UserEmailNotification(IEmailService emailService, IUserCreatedEmailContentBuilder userCreatedEmailContentBuilder)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService)); ;
            _userCreatedEmailContentBuilder = userCreatedEmailContentBuilder ?? throw new ArgumentNullException(nameof(userCreatedEmailContentBuilder)); ;
        }
        public async Task SendEmailAsync(Users user, int userId, string Password, UserStatus status)
        {
            switch(status)
            {
                case UserStatus.New:
                    var content = _userCreatedEmailContentBuilder.BuildContentforNewUser(user, userId, Password);
                    await _emailService.SendEmailAsync(user.Email, MailMessages.UserOnboarding.WelcomeMessages, content); // "Welcome to Task Management System – Your Account Details", content);
                    break;
                case UserStatus.PasswordReset:
                    var emailContent = _userCreatedEmailContentBuilder.BuildContentforPasswordReset(user, userId, Password);
                    await _emailService.SendEmailAsync(user.Email, MailMessages.UserOnboarding.WelcomeMessageReset, emailContent);// "Reset Password – Your Account Details", emailContent);
                    break;
                default:
                    return;

            }
            

        }
    }
}
