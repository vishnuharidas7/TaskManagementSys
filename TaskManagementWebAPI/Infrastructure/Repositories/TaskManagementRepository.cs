using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            catch(Exception ex)
            {
                _logger.LoggWarning("ViewUsers-Viewuser failed");
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
            catch(Exception ex)
            {
                _logger.LoggWarning("VieAllTask-ViewAllTask failed");
                throw;
            }
        }

        public async Task AddTask(AddTaskDTO dto)
        {
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
            catch(Exception ex)
            {
                _logger.LoggWarning("AddTask-Save failed");
                throw;
            }
        }

        public async Task<string>GenerateUniqueNumericIDTaskAsync(string prefix)
        {
            //string IDTask;
            //var random=new Random();
            //do
            //{
            //    int number = random.Next(100, 999999);
            //    IDTask = $"{prefix}-{number}";
            //}
            //while (await _db.Task.AnyAsync(t => t.referenceId == IDTask));
            //return IDTask;

            // Ensure prefix is followed by "-" like "TMS-"
            string searchPrefix = prefix + "-";

            var lastTask = await _db.Task
                .Where(t => t.referenceId.StartsWith(searchPrefix))
                .OrderByDescending(t => t.referenceId)
                .FirstOrDefaultAsync();

            int nextNumber = 1000; // start from 12000 if no task exists

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
        //            // return;
        //            throw new ArgumentException("All task due dates are either today or in the past. Please upload valid tasks.");
        //        }

        //        var useDapper = _configuration.GetValue<bool>("UseDapper:UseDapper");
        //        if(useDapper)
        //        {
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
        //                transaction.Dispose();
        //                _connection.Close();
        //            }
        //        }
        //        else
        //        {
        //            _db.Task.AddRange(tasks);
        //            await _db.SaveChangesAsync();
        //        }

        //        var tasksByUser = tasks
        //            .GroupBy(t => t.UserId)
        //            .ToDictionary(g => g.Key, g => g.ToList());

        //        foreach (var entry in tasksByUser)
        //        {
        //            var userId = entry.Key;
        //            var userTasks = entry.Value;

        //            Users? user = null;

        //            if (useDapper)
        //            {
        //                _connection.Open();
        //              using var transaction = _connection.BeginTransaction();
        //                try
        //                {
        //                    user = await _dapper.GetUserByIdAsync(userId, transaction);
        //                }
        //                catch
        //                {
        //                    _connection.Close();
        //                    throw;
        //                }
        //                finally
        //                {
        //                    _connection.Close();
        //                }

        //            }
        //            else
        //            {
        //                 user = await _db.User.FindAsync(userId);
        //            }

        //            if (user == null)
        //            {
        //                _logger.LoggWarning("User not found for ID {UserId} during task upload", userId);
        //                continue;
        //            }

        //            var content = _contentBuilder.BuildContent(user, userTasks);

        //            await _emailService.SendEmailAsync(
        //                user.Email,
        //                "New Tasks Assigned to You",
        //                content
        //            );
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LoggWarning("File upload failed");
        //        throw;
        //    }
        //}
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
            catch (Exception ex)
            {
                _logger.LoggWarning("File upload failed");
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
            catch(Exception ex)
            {
                _logger.LoggWarning("DeleteTask-Deletion failed");
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
                await _db.SaveChangesAsync();

                if(obj.taskStatus == "Completed")
                {
                    //var user = await _db.User.FindAsync(obj.UserId);

                    //if (user == null)
                    //{
                    //    _logger.LoggWarning("User not found for ID {UserId}", obj.UserId);
                    //    return;
                    //}

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
            catch(Exception ex)
            {
                _logger.LoggWarning("UpdateTask-Updation failed");
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
                        referenceId=t.referenceId
                    })
                    .ToListAsync();

                return tasks;
            }
            catch(Exception ex)
            {
                _logger.LoggWarning("GetTasksByUserId-Get data failed");
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
            catch (Exception ex)
            {
                _logger.LoggWarning("GetTasksNotificationByUserId-Get data failed");
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
            catch (Exception ex)
            {
                _logger.LoggWarning("GetTasksNotification-Get data failed");
                throw;
            }
        }
    }
}
