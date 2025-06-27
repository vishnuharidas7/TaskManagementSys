using Microsoft.AspNetCore.Connections;
using System.Data;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using Dapper;
using TaskEntity = TaskManagementWebAPI.Domain.Models.Tasks;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{
    public class TaskUploadDapperRepository : ITaskUploadDapperRepository
    {
        private readonly IDapperConnectionFactory _connectionFactory;
        public TaskUploadDapperRepository(IDapperConnectionFactory db) 
        {
            _connectionFactory = db;
        }

        
        public async Task InsertTasksAsync(IEnumerable<TaskEntity> tasks, IDbTransaction transaction)
        {
            var connection = transaction.Connection;  
            if (connection.State != ConnectionState.Open)
                connection.Open();

            string sql = @"
                INSERT INTO Task (taskName, taskDescription, taskStatus, createdDate,createdBy, dueDate, UserId, priority, taskState, taskType)
                VALUES (@taskName, @taskDescription, @taskStatus, @createdDate,@createdBy, @dueDate, @UserId, @priority, @taskState, @taskType);";

            foreach (var task in tasks)
            {
                await connection.ExecuteAsync(sql, task, transaction);
            }
        }

        public async Task<Users> GetUserByIdAsync(int userId, IDbTransaction? transaction=null)
        {
            using var connection = _connectionFactory.CreateConnection();  

            string sql = "SELECT * FROM `User` WHERE UserId = @UserId";
            return await connection.QueryFirstOrDefaultAsync<Users>(sql, new { UserId = userId }, transaction);
        }

    }
}
