using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common.ExceptionMessages;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskEntity = TaskManagementWebAPI.Domain.Models.Tasks;

namespace TaskManagementWebAPI.Application.Services.FileUpload
{
    public class TaskUploadDapperRepository : ITaskUploadDapperRepository
    {
        private readonly IDapperConnectionFactory _connectionFactory;
        private readonly TaskSettings _taskSettings;
        public TaskUploadDapperRepository(IDapperConnectionFactory db,IOptions<TaskSettings>taskSettings) 
        {
            _connectionFactory = db ?? throw new ArgumentNullException(nameof(db));
            _taskSettings =taskSettings.Value ?? throw new ArgumentNullException(nameof(taskSettings));
        }

        
        public async Task InsertTasksAsync(IEnumerable<TaskEntity> tasks, IDbTransaction transaction)
        {
            try
            {
                var connection = transaction.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string sql = @"
                INSERT INTO Task (taskName, taskDescription, taskStatus, createdDate,createdBy, dueDate, UserId, priority, taskState, taskType ,referenceId)
                VALUES (@taskName, @taskDescription, @taskStatus, @createdDate,@createdBy, @dueDate, @UserId, @priority, @taskState, @taskType, @referenceId);";

                int maxAttempts = 5;
                int attempt = 0;
                bool success = false;

                while (!success && attempt < maxAttempts)
                {
                    try
                    {
                        string lastUsedReferenceNo = await GenerateUniqueNumericIDTaskAsync(_taskSettings.IDTaskPrefix, transaction, connection);
                        int nextNumber = ExtractNumberFromReferenceId(lastUsedReferenceNo) + 1;

                        foreach (var task in tasks)
                        {
                            task.referenceId = $"{_taskSettings.IDTaskPrefix}-{nextNumber++}";
                            await connection.ExecuteAsync(sql, task, transaction);
                        }

                        success = true;
                    }
                    catch (DbException dbEx) when (IsDuplicateReferenceIdException(dbEx))
                    {
                        attempt++;
                        // Optional delay between retries
                        await Task.Delay(50);
                    }
                }

                if (!success)
                {
                    throw new InvalidOperationException(ExceptionMessages.TaskExceptions.ReferenceIdConflict);
                }
            }
            catch (DbException dbEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private bool IsDuplicateReferenceIdException(DbException ex)
        {
            return ex.Message.Contains("Duplicate entry") && ex.Message.Contains("referenceId");
        }

        public async Task<string> GenerateUniqueNumericIDTaskAsync(string prefix,IDbTransaction transaction,IDbConnection connection)
        {
            try
            {
                string searchPrefix = prefix + "-%";

                string sql = @"SELECT referenceId 
                         FROM Task
                         WHERE referenceId LIKE @Prefix
                         ORDER BY referenceId DESC
                         LIMIT 1;";

                return await connection.QueryFirstOrDefaultAsync<string>(sql, new { Prefix = searchPrefix }, transaction) ?? $"{prefix}-{_taskSettings.InitialReferenceId}";
            }
            catch (DbException dbEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private int ExtractNumberFromReferenceId(string referenceId)
        {
            var parts = referenceId.Split('-');
            return parts.Length == 2 && int.TryParse(parts[1], out int number) ? number : _taskSettings.InitialReferenceId;
        }


        public async Task<Users> GetUserByIdAsync(int userId, IDbTransaction? transaction)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();

                string sql = "SELECT * FROM `User` WHERE UserId = @UserId";
                return await connection.QueryFirstOrDefaultAsync<Users>(sql, new { UserId = userId }, transaction);
            }
            catch (DbException dbEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
