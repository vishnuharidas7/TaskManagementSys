using Dapper;
using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml.Linq; 
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 
using TaskManagementWebAPI.Infrastructure.Persistence;


namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class TaskManagementRepository : ITaskManagementRepository
    { 
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<UserAuthRepository> _logger;
        private readonly ITaskUploadDapperRepository _dapper;
        private readonly IDbConnection _connection; 

        public TaskManagementRepository(ApplicationDbContext db, IAppLogger<UserAuthRepository> logger, 
            ITaskUploadDapperRepository dapper, IDbConnection connection)
        { 
            _db = db; 
            _logger = logger; 
            _dapper = dapper;
            _connection = connection; 
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
                    referenceId=u.referenceId
                })
                .ToListAsync();

                return viewAlltasks;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "ViewAllTasks - Invalid operation while querying tasks.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "ViewAllTasks - Database error while fetching tasks.");
                throw ex.InnerException;
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


        //[Obsolete]
        //public async Task ProcessFileAsync(IFormFile file)
        //{
        //    try
        //    {
        //        var parser = _parserFactory.GetParser(file.FileName);
        //        var rawData = await parser.ParseAsync(file);

        //        var tasks = _taskMapper.MapToTasks(rawData);

        //        var tomorrow = DateTime.Today.AddDays(1);

        //        var validTasks = tasks
        //            .Where(t => t.dueDate.Date >= tomorrow)
        //            .ToList();

        //        if (!validTasks.Any())
        //        {
        //            throw new ArgumentException("All task due dates are either today or in the past. Please upload valid tasks.");
        //        }

        //        var useDapper = _configuration.GetValue<bool>("UseDapper:UseDapper");

        //        if (useDapper)
        //        {
        //            // --- DAPPER BLOCK ---
        //            _connection.Open();
        //            using var transaction = _connection.BeginTransaction();
        //            try
        //            {
        //                await _dapper.InsertTasksAsync(validTasks, transaction);
        //                transaction.Commit();
        //            }
        //            catch
        //            {
        //                transaction.Rollback();
        //                throw;
        //            }
        //            finally
        //            {
        //                _connection.Close();
        //            }

        //            var tasksByUser = validTasks
        //                .GroupBy(t => t.UserId)
        //                .ToDictionary(g => g.Key, g => g.ToList());

        //            _connection.Open(); 

        //            foreach (var entry in tasksByUser)
        //            {
        //                var userId = entry.Key;
        //                var userTasks = entry.Value;

        //                Users? user = null;

        //                try
        //                { 
        //                    user = await _dapper.GetUserByIdAsync(userId, null);

        //                }
        //                catch
        //                {
        //                    _connection.Close();
        //                    throw;
        //                } 

        //                if (user == null)
        //                {
        //                    _logger.LoggWarning("User not found for ID {UserId} during task upload", userId);
        //                    continue;
        //                }

        //                var content = _contentBuilder.BuildContent(user, userTasks);
        //                await _emailService.SendEmailAsync(user.Email, "New Tasks Assigned to You", content);
        //            }

        //            _connection.Close();
        //        }
        //        else
        //        {
        //            // --- EF CORE BLOCK ---
        //            string lastRefId = await GetLastReferenceIdEFAsyncUploadEF(_taskSettings.IDTaskPrefix);
        //            int nextNumber = ExtractNumberFromReferenceIdUploadEF(lastRefId) + 1;
        //            foreach (var task in validTasks)
        //            {
        //                task.referenceId = $"{_taskSettings.IDTaskPrefix}-{nextNumber}";
        //                nextNumber++;

        //            }

        //            _db.Task.AddRange(validTasks);
        //            await _db.SaveChangesAsync();

        //            var tasksByUser = validTasks
        //                .GroupBy(t => t.UserId)
        //                .ToDictionary(g => g.Key, g => g.ToList());

        //            foreach (var entry in tasksByUser)
        //            {
        //                var userId = entry.Key;
        //                var userTasks = entry.Value;

        //                var user = await _db.User.FindAsync(userId);
        //                if (user == null)
        //                {
        //                    _logger.LoggWarning("User not found for ID {UserId} during task upload", userId);
        //                    continue;
        //                }

        //                var content = _contentBuilder.BuildContent(user, userTasks);
        //                await _emailService.SendEmailAsync(user.Email, "New Tasks Assigned to You", content);
        //            }
        //        }
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LoggWarning("Validation failed during file processing: {Message}", ex.Message);
        //        throw;
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LoggError(ex, "Invalid operation while processing file.");
        //        throw;
        //    }
        //    catch (SqlException ex)
        //    {
        //        _logger.LoggError(ex, "SQL error occurred during file processing.");
        //        throw;
        //    }
        //    catch (IOException ex)
        //    {
        //        _logger.LoggError(ex, "File I/O error occurred during file processing.");
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LoggError(ex, "Unexpected error occurred during task file upload.");
        //        throw;
        //    }
        //}


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
            string lastRefId = await GetLastReferenceIdEFAsyncUploadEF(prefix);
            int nextNumber = ExtractNumberFromReferenceIdUploadEF(lastRefId) + 1;

            foreach (var task in tasks)
            {
                task.referenceId = $"{prefix}-{nextNumber}";
                nextNumber++;
            }

            _db.Task.AddRange(tasks);
            await _db.SaveChangesAsync();
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
                    throw new Exception("Task not found");
                }
                _db.Task.Remove(task);
                await _db.SaveChangesAsync();
            }
            catch (KeyNotFoundException)
            {
                throw;
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
                               
            }
            catch (KeyNotFoundException)
            {
                
                throw;
            }
            catch (ApplicationException)
            {
                
                throw;
            }
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
    }
}
