using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskEntity = TaskManagementWebAPI.Domain.Models.Tasks;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{
    public class TaskUploadDapperRepository : ITaskUploadDapperRepository
    {
        private readonly IDapperConnectionFactory _connectionFactory;
      //  private readonly ApplicationDbContext _db;
        private readonly TaskSettings _taskSettings;
        public TaskUploadDapperRepository(IDapperConnectionFactory db,IOptions<TaskSettings>taskSettings) 
        {
            _connectionFactory = db;
            _taskSettings=taskSettings.Value;
        }

        
        public async Task InsertTasksAsync(IEnumerable<TaskEntity> tasks, IDbTransaction transaction)
        {
            var connection = transaction.Connection;  
            if (connection.State != ConnectionState.Open)
                connection.Open();

            string sql = @"
                INSERT INTO Task (taskName, taskDescription, taskStatus, createdDate,createdBy, dueDate, UserId, priority, taskState, taskType ,referenceId)
                VALUES (@taskName, @taskDescription, @taskStatus, @createdDate,@createdBy, @dueDate, @UserId, @priority, @taskState, @taskType, @referenceId);";

            string lastUsedReferenceNo= await GenerateUniqueNumericIDTaskAsync(_taskSettings.IDTaskPrefix,transaction,connection);
            int nextNumber = ExtractNumberFromReferenceId(lastUsedReferenceNo)+1;

            foreach (var task in tasks)
            {
               task.referenceId= $"{_taskSettings.IDTaskPrefix}-{nextNumber}";
                nextNumber++;

                await connection.ExecuteAsync(sql, task, transaction);
            }
        }

        public async Task<string> GenerateUniqueNumericIDTaskAsync(string prefix,IDbTransaction transaction,IDbConnection connection)
        {
            string searchPrefix = prefix + "-%";

            string sql = @"SELECT referenceId 
                         FROM Task
                         WHERE referenceId LIKE @Prefix
                         ORDER BY referenceId DESC
                         LIMIT 1;";

            return await connection.QueryFirstOrDefaultAsync<string>(sql, new { Prefix = searchPrefix }, transaction) ?? $"{prefix}-{_taskSettings.InitialReferenceId}";

        }

        private int ExtractNumberFromReferenceId(string referenceId)
        {
            var parts = referenceId.Split('-');
            return (parts.Length == 2 && int.TryParse(parts[1], out int number)) ? number : _taskSettings.InitialReferenceId;
        }


        public async Task<Users> GetUserByIdAsync(int userId, IDbTransaction? transaction=null)
        {
            using var connection = _connectionFactory.CreateConnection();  

            string sql = "SELECT * FROM `User` WHERE UserId = @UserId";
            return await connection.QueryFirstOrDefaultAsync<Users>(sql, new { UserId = userId }, transaction);
        }

    }
}
