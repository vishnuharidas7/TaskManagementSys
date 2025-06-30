using System.Data;
using TaskManagementWebAPI.Domain.Models;
using TaskEntity = TaskManagementWebAPI.Domain.Models.Tasks;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskUploadDapperRepository
    {
        Task InsertTasksAsync(IEnumerable<TaskEntity> tasks, IDbTransaction transaction);
        Task<Users> GetUserByIdAsync(int userId, IDbTransaction transaction);
    }
}
