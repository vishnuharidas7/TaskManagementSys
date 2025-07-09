using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface IEmailContentBuilder
    {
        /// <summary>
        /// For building Email content
        /// </summary>
        /// <param name="user"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        string BuildContent(Users user, IEnumerable<Tasks> task);
    }
}
