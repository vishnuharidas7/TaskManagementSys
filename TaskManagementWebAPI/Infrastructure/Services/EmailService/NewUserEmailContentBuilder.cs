using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class NewUserEmailContentBuilder : INewUserEmailContentBuilder
    {
        //private readonly IEnumerable<ITaskStatusContentBuilder> _userBuilders;

        //public UserRegisterEmailContentBuilder(IEnumerable<ITaskStatusContentBuilder> userBuilders)
        //{
        //    _userBuilders = userBuilders;
        //}
        private readonly IAppLogger<NewUserEmailContentBuilder> _logger;

        public NewUserEmailContentBuilder(IAppLogger<NewUserEmailContentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string BuildContentforNewUser(Users user, int userId, string Password)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Hey {user.Name},\n");

                sb.AppendLine("Welcome to Task Management System! Your account has been created successfully\n");
                sb.AppendLine("Your login credentials : \n");

                sb.AppendLine($"Username: {user.UserName}\n");
                sb.AppendLine($"Email: {user.Email}\n");
                sb.AppendLine($"password: {Password}\n");


                sb.AppendLine("For security reasons, please update your password once you've signed in.\n");

                sb.AppendLine("Regards,\nTask Management System");

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
                sb.AppendLine($"Hey {user.Name},\n");

                sb.AppendLine("Your account password has been reset successfully\n");
                sb.AppendLine("Your login credentials : \n");

                sb.AppendLine($"Username: {user.UserName}\n");
                sb.AppendLine($"Email: {user.Email}\n");
                sb.AppendLine($"password: {NewPassword}\n");


                sb.AppendLine("For security reasons, please update your password once you've signed in.\n");

                sb.AppendLine("Regards,\nTask Management System");

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
