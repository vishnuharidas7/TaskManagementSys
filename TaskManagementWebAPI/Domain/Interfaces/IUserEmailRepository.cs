using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserEmailRepository
    {
        Users GetUserById(int id);
        IEnumerable<Users> GetAllUsers();
    }
}
