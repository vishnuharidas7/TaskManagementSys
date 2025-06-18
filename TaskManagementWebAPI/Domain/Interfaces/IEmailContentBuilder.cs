using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IEmailContentBuilder
    {
        string BuildContent(Users user, IEnumerable<Tasks> task);
    }
}
