using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories;

namespace TaskManagementWebAPI.Application.Services
{
    public class TaskApplicartionService : ITaskApplicationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITaskManagementRepository _taskManagementRepository;
        private readonly IAppLogger<UserAuthRepository> _logger;
        private readonly TaskSettings _taskSettings;
        private readonly IEmailContentBuilder _contentBuilder;
        private readonly IEmailService _emailService;
        private readonly ITaskFileParserFactory _parserFactory;
        private readonly IMaptoTasks _taskMapper;
        private readonly IConfiguration _configuration;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public TaskApplicartionService(ITaskManagementRepository taskManagementRepository, IAppLogger<UserAuthRepository> logger,
          IOptions<TaskSettings> taskSettings, ApplicationDbContext db, IEmailContentBuilder emailContentBuilder,
          IEmailService emailService, ITaskFileParserFactory parserFactory, IMaptoTasks taskMapper, IConfiguration configuration)
        {
            _db = db;
            _taskManagementRepository = taskManagementRepository;
            _logger = logger;
            _taskSettings = taskSettings.Value;
            _emailService = emailService;
            _contentBuilder = emailContentBuilder;
            _parserFactory = parserFactory;
            _taskMapper = taskMapper;
            _configuration = configuration;
        }

        public async Task AddTaskAsync(AddTaskDTO dto)
        {
            await _semaphore.WaitAsync();
            try
            {
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
                    
                };

                var newTaskId = await _taskManagementRepository.AddTask(task);
                if (newTaskId != null)
                {
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

        public async Task<string> GenerateUniqueNumericIDTaskAsync(string prefix)
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
                    await _taskManagementRepository.UpdateTask(task); //_db.SaveChangesAsync();
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
                    throw new ArgumentException("All task due dates are either today or in the past.");
                }

                var useDapper = _configuration.GetValue<bool>("UseDapper:UseDapper");

                if (useDapper)
                {
                    await _taskManagementRepository.SaveTasksWithDapperAsync(validTasks);
                }
                else
                {
                    await _taskManagementRepository.SaveTasksWithEFAsync(validTasks, _taskSettings.IDTaskPrefix);
                }

                var tasksByUser = validTasks
                    .GroupBy(t => t.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var entry in tasksByUser)
                {
                    var userId = entry.Key;
                    var userTasks = entry.Value;

                    var user = await _db.User.FindAsync(userId);
                    //var user = await _db.User.FindAsync(entry.Key);
                    if (user == null)
                    {
                        _logger.LoggWarning("User not found for ID {UserId}", entry.Key);
                        continue;
                    }

                    var content = _contentBuilder.BuildContent(user, entry.Value);
                    await _emailService.SendEmailAsync(user.Email, "New Tasks Assigned to You", content);
                }
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Error processing task file.");
                throw;
            }
        }

    }
}
