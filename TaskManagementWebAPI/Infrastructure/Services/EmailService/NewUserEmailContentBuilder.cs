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

        public string BuildContent(Users user, int userId, string Password)
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
    }
}
