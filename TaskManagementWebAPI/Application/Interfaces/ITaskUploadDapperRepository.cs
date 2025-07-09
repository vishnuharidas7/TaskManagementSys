using System.Data;
using TaskManagementWebAPI.Domain.Models;
using TaskEntity = TaskManagementWebAPI.Domain.Models.Tasks;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskUploadDapperRepository
    {
        /// <summary>
        /// To insert data using dapper query
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task InsertTasksAsync(IEnumerable<TaskEntity> tasks, IDbTransaction transaction);
        /// <summary>
        /// To get user details
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<Users> GetUserByIdAsync(int userId, IDbTransaction transaction);
    }
}
