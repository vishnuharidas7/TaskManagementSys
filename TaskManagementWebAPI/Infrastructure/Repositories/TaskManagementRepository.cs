using Dapper;
using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SendGrid.Helpers.Errors.Model;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq; 
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common.ExceptionMessages;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 
using TaskManagementWebAPI.Infrastructure.Persistence;


namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class TaskManagementRepository : ITaskManagementRepository
    { 
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<TaskManagementRepository> _logger;
        private readonly ITaskUploadDapperRepository _dapper;
        private readonly IDbConnection _connection; 

        public TaskManagementRepository(ApplicationDbContext db, IAppLogger<TaskManagementRepository> logger, 
            ITaskUploadDapperRepository dapper, IDbConnection connection)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public  async Task<List<Tasks>>GetTasksByTaskIdAsync(int taskId)
        {
            try
            {
                var getTasksByTaskIdAsync = await _db.Task.Where(t => t.taskId == taskId).ToListAsync();
                return getTasksByTaskIdAsync;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "GetTasksByTaskIdAsync - Invalid operation while querying Tasks.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "GetTasksByTaskIdAsync - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "GetTasksByTaskIdAsync - An unexpected error occurred.");
                throw;
            }
        }

        public async Task<Tasks> TaskWithIdFindAsync(int taskId)
        {
            try
            {
                var task = await _db.Task.FindAsync(taskId);
                return task;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "TaskWithIdFindAsync - Invalid operation while querying Tasks.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "TaskWithIdFindAsync - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "TaskWithIdFindAsync - An unexpected error occurred.");
                throw;
            }
        }
       
        public async Task<Tasks> LastTaskWithPrefix(string prefix)
        {
            try
            {
                var lastTaskWithPrefix = await _db.Task
                   .Where(t => t.referenceId.StartsWith(prefix))
                   .OrderByDescending(t => t.referenceId)
                   .FirstOrDefaultAsync();
                return lastTaskWithPrefix;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "LastTaskWithPrefix - Invalid operation while querying Last task.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "LastTaskWithPrefix - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "LastTaskWithPrefix - An unexpected error occurred.");
                throw;
            }

        }
        public async Task<List<AssignUserDTO>> ViewUsers()
        {
            try
            {

                var usersWithRoles = await _db.User
                .Include(u => u.Role)
                .Select(u => new AssignUserDTO
                {
                    Id = u.UserId,
                    Name = u.Name
                })
                .ToListAsync();

                return usersWithRoles;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "ViewUsers - Invalid operation while querying users.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "ViewUsers - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "ViewUsers - An unexpected error occurred.");
                throw;
            }
        }

        public async Task<List<ViewTasksDTO>> viewAllTasks()
        {
            try
            { 
                var viewAlltasks = await _db.Task
                .Include(u => u.User)
                .Select(u => new ViewTasksDTO
                {
                    taskId = u.taskId,
                    taskName = u.taskName,
                    userName = u.User.Name,
                    userId = u.UserId,
                    dueDate = u.dueDate,
                    taskDescription = u.taskDescription,
                    taskStatus = u.taskStatus,
                    priority = u.priority,
                    taskType = u.taskType,
                    referenceId=u.referenceId,
                    taskState = u.taskState
                })
                .ToListAsync();

                return viewAlltasks;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "ViewAllTasks - Invalid operation while querying tasks.");
                throw;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "ViewAllTasks - Database error while fetching tasks.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "ViewAllTasks - Unexpected error.");
                throw;
            }
        }

        public async Task<int> AddTask(Tasks task)
        { 
            try { 

                _db.Task.Add(task);
                await _db.SaveChangesAsync();
                return task.taskId;
                 
            } 
            catch (DbUpdateException ex)
            {
                _logger.LoggError(ex, "AddTask - Database update failed while saving task");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "AddTask - Database update failed while saving task");
                throw;
            } 
        }



        public async Task SaveTasksWithDapperAsync(List<Tasks> tasks)
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                await _dapper.InsertTasksAsync(tasks, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task SaveTasksWithEFAsync(List<Tasks> tasks, string prefix)
        {
            const int maxAttempts = 5;
            int attempt = 0;
            bool saved = false;
            while (attempt < maxAttempts && !saved)
            {

                attempt++;
                try
                {
                    string lastRefId = await GetLastReferenceIdEFAsyncUploadEF(prefix);
                    int nextNumber = ExtractNumberFromReferenceIdUploadEF(lastRefId) + 1;
                    foreach (var task in tasks)
                    {
                        task.referenceId = $"{prefix}-{nextNumber}";
                        nextNumber++;

                    }

                    _db.Task.AddRange(tasks);
                    await _db.SaveChangesAsync();
                    saved = true;

                }
                catch (DbUpdateException ex) when (IsDuplicateReferenceIdException(ex))
                {
                    _logger.LoggWarning("Attempt {Attempt}: Duplicate referenceId. Retrying...", attempt);
                    await Task.Delay(100);
                }

            }
            if (!saved)
            {
                throw new Exception(ExceptionMessages.TaskExceptions.ReferenceIdConflict);
            }
        }
        private bool IsDuplicateReferenceIdException(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("Duplicate entry") == true
                && ex.InnerException?.Message.Contains("referenceId") == true;
        }
        public async Task<string> GetLastReferenceIdEFAsyncUploadEF(string prefix)
        {
            try
            {
                string searchPrefix = prefix + "-";

                var lastTask = await _db.Task
                    .Where(t => t.referenceId.StartsWith(searchPrefix))
                    .OrderByDescending(t => t.referenceId)
                    .FirstOrDefaultAsync();

                return lastTask?.referenceId ?? $"{prefix}-1000"; // fallback if none found
            }
            catch (SqlException ex)
            {
                _logger.LoggError(ex, "Database error occurred while getting last reference ID with prefix {Prefix}", prefix);
                throw;
            }
            catch (IOException ex)
            {
                _logger.LoggError(ex, "I/O error occurred while getting last reference ID with prefix {Prefix}", prefix);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "Invalid operation while getting last reference ID with prefix {Prefix}", prefix);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred while getting last reference ID with prefix {Prefix}", prefix);
                throw;
            }

        }
        public int ExtractNumberFromReferenceIdUploadEF(string referenceId)
        {
            try
            {
                var parts = referenceId.Split('-');
                return (parts.Length == 2 && int.TryParse(parts[1], out int number)) ? number : 1000;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LoggError(ex, "ReferenceId was null in ExtractNumberFromReferenceIdUploadEF");
                throw;
            }
            catch (FormatException ex)
            {
                _logger.LoggError(ex, "ReferenceId format invalid in ExtractNumberFromReferenceIdUploadEF");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error in ExtractNumberFromReferenceIdUploadEF");
                throw;
            }
        }




        public async Task DeleteTask(int id)
        {
            try
            {
                var task = await _db.Task.FindAsync(id);
                if (task == null)
                {
                    throw new NotFoundException($"Task with ID {id} not found.");
                }
                _db.Task.Remove(task);
                await _db.SaveChangesAsync();
            }
           
            catch (DbUpdateException dbEx)
            {
                _logger.LoggError(dbEx, "Database update error while deleting task with ID {TaskId}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred while deleting task with ID {TaskId}.", id);
                throw;
            }

        }

        public async Task UpdateTask(Tasks task)
        {
                try
                {
                    _db.Task.Update(task);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LoggWarning("Database error while retrieving user/tasks: {Message}", dbEx.Message);
                    throw dbEx.InnerException;
                }
            //catch (ApplicationException)
            //{
                
            //    throw;
            //}
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred while updating task with ID {TaskId}.", task.taskId);
                throw;
            }
        }
 
        public async Task<IEnumerable<ViewTasksDTO>> GetTasksByUserId(int userId)
        {
            try
            {
                var tasks = await _db.Task
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId)
                    .Select(t => new ViewTasksDTO
                    {
                        taskId = t.taskId,
                        taskName = t.taskName,
                        taskDescription = t.taskDescription,
                        taskStatus = t.taskStatus,
                        dueDate = t.dueDate,
                        priority = t.priority,
                        userId = t.UserId,
                        userName = t.User.UserName,
                        taskType = t.taskType,
                        referenceId=t.referenceId
                    })
                    .ToListAsync();
                if (tasks == null)
                {
                    throw new NotFoundException(string.Format(ExceptionMessages.TaskExceptions.TaskNotFoundByUserId, userId));
                }

                return tasks;
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LoggError(argEx, "GetTasksByUserId failed due to null argument.");
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LoggError(invOpEx, "GetTasksByUserId failed due to invalid operation.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred in GetTasksByUserId.");
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetTasksNotificationByUserId(int userId)
        {
            try
            {
                var today = DateTime.Now;
                var tasks = await _db.Task
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId && 
                    t.dueDate<= today.AddDays(2) && (t.taskStatus=="In-Progress" || t.taskStatus=="New" || t.taskStatus == "Blocked"))
                    .Select(t => new NotificationDTO
                    {
                        TaskId = t.taskId,
                        TaskName = t.taskName,
                        TaskStatus = t.taskStatus,
                        DueDate = t.dueDate,
                        referenceId=t.referenceId
                    })
                    .ToListAsync();
                //if (tasks == null)
                //{
                //    throw new NotFoundException($"Task with User ID {userId} not found.");
                //}

                return tasks;
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LoggError(argEx, "GetTasksNotificationByUserId failed due to null argument.");
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LoggError(invOpEx, "GetTasksNotificationByUserId failed due to invalid operation.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred in GetTasksNotificationByUserId.");
                throw;
            }
        }

        public async Task<List<NotificationDTO>> GetTasksNotificationbByAdmin()
        {
            try
            {
                var today = DateTime.Now;
                var tasks = await _db.Task
            .Include(t => t.User)
            .Where(t =>
                (t.taskStatus == "In-Progress" || t.taskStatus == "New" || t.taskStatus == "Blocked") &&
                t.dueDate <= today.AddDays(2))
            .Select(t => new NotificationDTO
            {
                TaskId = t.taskId,
                TaskName = t.taskName,
                TaskStatus = t.taskStatus,
                DueDate = t.dueDate,
                UserName = t.User.UserName,
                referenceId=t.referenceId
            })
            .ToListAsync();

                //if (tasks == null)
                //{
                //    throw new NotFoundException($"Task with User ID {userId} not found.");
                //}
                return tasks;
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LoggError(argEx, "GetTasksNotificationByAdmin failed due to null argument.");
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LoggError(invOpEx, "GetTasksNotificationByAdmin failed due to invalid operation.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred in GetTasksNotificationByAdmin.");
                throw;
            }
        }

        public IEnumerable<Tasks>GetAllTasksByUserId(int userId)
        {
            try
            {
                return _db.Task
                .Where(t => t.UserId == userId)
                .ToList();
            }
            catch (ArgumentNullException argEx)
            {
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
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
