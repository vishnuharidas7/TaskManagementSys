using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class UserCreatedEmailContentBuilder : IUserCreatedEmailContentBuilder
    {
        //public UserRegisterEmailContentBuilder(IEnumerable<ITaskStatusContentBuilder> userBuilders)
        //{
        //    _userBuilders = userBuilders;
        //}
        private readonly IAppLogger<UserCreatedEmailContentBuilder> _logger;

        public UserCreatedEmailContentBuilder(IAppLogger<UserCreatedEmailContentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string BuildContentforNewUser(Users user, int userId, string Password)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Format(MailMessages.UserOnboarding.Greeting, user.Name));
                sb.AppendLine(MailMessages.UserOnboarding.WelcomeMessageReset);
                sb.AppendLine(MailMessages.UserOnboarding.CredentialsIntro);

                sb.AppendLine(string.Format(MailMessages.UserOnboarding.UsernameLine, user.UserName));
                sb.AppendLine(string.Format(MailMessages.UserOnboarding.EmailLine, user.Email));
                sb.AppendLine(string.Format(MailMessages.UserOnboarding.PasswordLine, Password));

                sb.AppendLine(MailMessages.UserOnboarding.PasswordReminder);

                sb.AppendLine(MailMessages.Signature);

                return sb.ToString();
            }

            catch (Exception ex)
            {
                _logger.LoggError(ex, "Failed to build new user email content.");
                throw;
            }

        }

        public string BuildContentforPasswordReset(Users user, int userId, string NewPassword)
        {
            try
            {

                var sb = new StringBuilder();
                sb.AppendLine(string.Format(MailMessages.UserOnboarding.Greeting, user.Name));

                sb.AppendLine(MailMessages.UserOnboarding.WelcomeMessageReset);
                sb.AppendLine(MailMessages.UserOnboarding.CredentialsIntro);

                sb.AppendLine(string.Format(MailMessages.UserOnboarding.UsernameLine, user.UserName));
                sb.AppendLine(string.Format(MailMessages.UserOnboarding.EmailLine, user.Email));
                sb.AppendLine(string.Format(MailMessages.UserOnboarding.PasswordLine, NewPassword));


                sb.AppendLine(MailMessages.UserOnboarding.PasswordReminder);

                sb.AppendLine(MailMessages.Signature);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Failed to build account password content.");
                throw;
            }

        }
    }
}
