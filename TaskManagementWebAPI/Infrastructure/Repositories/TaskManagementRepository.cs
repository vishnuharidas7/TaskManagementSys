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
        private readonly ITaskFileParserFactory _parserFactory;
        private readonly IMaptoTasks _taskMapper;
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<UserAuthRepository> _logger;
        private readonly IEmailContentBuilder _contentBuilder;
        private readonly IEmailService _emailService;
        private readonly ITaskUploadDapperRepository _dapper;
        private readonly IDbConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly TaskSettings _taskSettings;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TaskManagementRepository(ITaskFileParserFactory parseFactory, ApplicationDbContext db,
            IMaptoTasks taskMapper, IAppLogger<UserAuthRepository> logger,
            IEmailContentBuilder contentBuilder, IEmailService emailService,
            ITaskUploadDapperRepository dapper, IDbConnection connection, IConfiguration configuration,
            IOptions<TaskSettings> taskSettings)
        {
            _parserFactory = parseFactory;
            _db = db;
            _taskMapper = taskMapper;
            _logger = logger;
            _contentBuilder = contentBuilder;
            _emailService = emailService;
            _dapper = dapper;
            _connection = connection;
            _configuration = configuration;
            _taskSettings = taskSettings.Value;
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

        public async Task AddTask(AddTaskDTO dto)
        {
            await _semaphore.WaitAsync();
            try {
                var task = new Tasks
                {
                    taskName = dto.taskName,
                    taskDescription = dto.taskDescription,
                    UserId = dto.UserId,
                    dueDate = dto.dueDate,
                    priority = dto.priority,
                    createdBy = dto.createdBy,
                    taskType = dto.taskType,
                    referenceId = await GenerateUniqueNumericIDTaskAsync(_taskSettings.IDTaskPrefix)
                    //taskStatus = dto.taskStatus

                };

                _db.Task.Add(task);
                await _db.SaveChangesAsync();
                var newTaskId = task.taskId;
                // return user;

                var user = await _db.User.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LoggWarning("User not found for ID {UserId}", dto.UserId);
                    return;
                }
                 
                var userTasks = await _db.Task
                                         .Where(t => t.taskId == newTaskId)
                                         .ToListAsync();
                if (userTasks.Any())
                {
                    var content = _contentBuilder.BuildContent(user, userTasks);
                    await _emailService.SendEmailAsync(user.Email, "New Task Added", content);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "AddTask - Invalid operation for user ID {UserId}", dto.UserId);
                throw ex.InnerException;
            }
            catch (DbUpdateException ex)
            {
                _logger.LoggError(ex, "AddTask - Database update failed while saving task for user ID {UserId}", dto.UserId);
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "AddTask - Unexpected error occurred while adding task for user ID {UserId}", dto.UserId);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string>GenerateUniqueNumericIDTaskAsync(string prefix)
        {
            try
            {

                string searchPrefix = prefix + "-";

                var lastTask = await _db.Task
                    .Where(t => t.referenceId.StartsWith(searchPrefix))
                    .OrderByDescending(t => t.referenceId)
                    .FirstOrDefaultAsync();

                int nextNumber = _taskSettings.InitialReferenceId; // start from 12000 if no task exists

                if (lastTask != null)
                {
                    string[] parts = lastTask.referenceId.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return $"{prefix}-{nextNumber}";

            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "GenerateUniqueNumericIDTaskAsync - Invalid operation while generating ID with prefix {Prefix}", prefix);
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "GenerateUniqueNumericIDTaskAsync - Unexpected error while generating ID with prefix {Prefix}", prefix);
                throw;
            }


        }

        [Obsolete]
        public async Task ProcessFileAsync(IFormFile file)
        {
            try
            {
                var parser = _parserFactory.GetParser(file.FileName);
                var rawData = await parser.ParseAsync(file);

                var tasks = _taskMapper.MapToTasks(rawData);

                var tomorrow = DateTime.Today.AddDays(1);

                var validTasks = tasks
                    .Where(t => t.dueDate.Date >= tomorrow)
                    .ToList();

                if (!validTasks.Any())
                {
                    throw new ArgumentException("All task due dates are either today or in the past. Please upload valid tasks.");
                }

                var useDapper = _configuration.GetValue<bool>("UseDapper:UseDapper");

                if (useDapper)
                {
                    // --- DAPPER BLOCK ---
                    _connection.Open();
                    using var transaction = _connection.BeginTransaction();
                    try
                    {
                        await _dapper.InsertTasksAsync(validTasks, transaction);
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

                    var tasksByUser = validTasks
                        .GroupBy(t => t.UserId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    _connection.Open(); 

                    foreach (var entry in tasksByUser)
                    {
                        var userId = entry.Key;
                        var userTasks = entry.Value;

                        Users? user = null;

                        try
                        { 
                            user = await _dapper.GetUserByIdAsync(userId, null);
                           
                        }
                        catch
                        {
                            _connection.Close();
                            throw;
                        } 

                        if (user == null)
                        {
                            _logger.LoggWarning("User not found for ID {UserId} during task upload", userId);
                            continue;
                        }

                        var content = _contentBuilder.BuildContent(user, userTasks);
                        await _emailService.SendEmailAsync(user.Email, "New Tasks Assigned to You", content);
                    }
                  
                    _connection.Close();
                }
                else
                {
                    // --- EF CORE BLOCK ---
                    string lastRefId = await GetLastReferenceIdEFAsyncUploadEF(_taskSettings.IDTaskPrefix);
                    int nextNumber = ExtractNumberFromReferenceIdUploadEF(lastRefId) + 1;
                    foreach (var task in validTasks)
                    {
                        task.referenceId = $"{_taskSettings.IDTaskPrefix}-{nextNumber}";
                        nextNumber++;

                    }

                    _db.Task.AddRange(validTasks);
                    await _db.SaveChangesAsync();

                    var tasksByUser = validTasks
                        .GroupBy(t => t.UserId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var entry in tasksByUser)
                    {
                        var userId = entry.Key;
                        var userTasks = entry.Value;

                        var user = await _db.User.FindAsync(userId);
                        if (user == null)
                        {
                            _logger.LoggWarning("User not found for ID {UserId} during task upload", userId);
                            continue;
                        }

                        var content = _contentBuilder.BuildContent(user, userTasks);
                        await _emailService.SendEmailAsync(user.Email, "New Tasks Assigned to You", content);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LoggWarning("Validation failed during file processing: {Message}", ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "Invalid operation while processing file.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LoggError(ex, "SQL error occurred during file processing.");
                throw;
            }
            catch (IOException ex)
            {
                _logger.LoggError(ex, "File I/O error occurred during file processing.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred during task file upload.");
                throw;
            }
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

        public async Task UpdateTask(int id, AddTaskDTO obj)
        {
            try
            {
                var task = await _db.Task.FindAsync(id);
                if (task == null)
                {
                    throw new Exception("Task not found");
                }

                task.taskName = obj.taskName;
                task.UserId = obj.UserId;
                task.dueDate = obj.dueDate;
                task.taskDescription = obj.taskDescription;
                task.taskStatus = obj.taskStatus;
                task.taskType = obj.taskType;
                if (obj.taskStatus == "Completed")
                {
                    task.taskState = obj.taskStatus;
                }

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LoggWarning("Database error while retrieving user/tasks: {Message}", dbEx.Message);
                    throw dbEx.InnerException;
                }


                if (obj.taskStatus == "Completed")
                {
                 
                    var userTasks = await _db.Task
                                             .Where(t => t.taskId == id)
                                             .ToListAsync();

                    var user = await _db.User.FindAsync(task.createdBy);

                    if (userTasks.Any())
                    {
                        var content = _contentBuilder.BuildContent(user, userTasks);
                        await _emailService.SendEmailAsync(user.Email, "Task Completed", content);
                    }
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
                _logger.LoggError(ex, "Unexpected error occurred while updating task with ID {TaskId}.", id);
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
