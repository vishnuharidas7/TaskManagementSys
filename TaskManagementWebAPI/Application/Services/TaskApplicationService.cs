using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NPOI.XWPF.UserModel;
using SendGrid.Helpers.Errors.Model;
using System.Threading;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Exceptions;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories;

namespace TaskManagementWebAPI.Application.Services
{
    public class TaskApplicationService : ITaskApplicationService
    {
        private readonly ITaskManagementRepository _taskManagementRepository;
        private readonly IAppLogger<TaskApplicationService> _logger;
        private readonly TaskSettings _taskSettings;
        private readonly IEmailContentBuilder _contentBuilder;
        private readonly IEmailService _emailService;
        private readonly ITaskFileParserFactory _parserFactory;
        private readonly IMaptoTasks _taskMapper;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public TaskApplicationService(ITaskManagementRepository taskManagementRepository, IAppLogger<TaskApplicationService> logger,
          IOptions<TaskSettings> taskSettings,IEmailContentBuilder emailContentBuilder,
          IEmailService emailService, ITaskFileParserFactory parserFactory,
          IMaptoTasks taskMapper, IConfiguration configuration, IUserRepository userRepository)
        {
            _taskManagementRepository = taskManagementRepository;
            _logger = logger;
            _taskSettings = taskSettings.Value;
            _emailService = emailService;
            _contentBuilder = emailContentBuilder;
            _parserFactory = parserFactory;
            _taskMapper = taskMapper;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task AddTaskAsync(AddTaskDTO dto)
        {
            int attempts = 0;
            const int maxAttempts = 5;
            await _semaphore.WaitAsync();
            try
            { 
                while(attempts<maxAttempts)
                {
                    try
                    {
                        var referenceId = await GenerateUniqueNumericIDTaskAsync(_taskSettings.IDTaskPrefix);
                        var task = new Tasks
                        {
                            taskName = dto.taskName,
                            taskDescription = dto.taskDescription,
                            UserId = dto.UserId,
                            dueDate = dto.dueDate,
                            priority = dto.priority,
                            createdBy = dto.createdBy,
                            taskType = dto.taskType,
                            referenceId = referenceId
                        };
                        var newTaskId = await _taskManagementRepository.AddTask(task);
                        if (newTaskId != null)
                        {
                            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
                            if (user == null)
                            {
                                _logger.LoggWarning("User not found for ID {UserId}", dto.UserId);
                                return;
                            }

                            var userTasks = await _taskManagementRepository.GetTasksByTaskIdAsync(newTaskId);
                            if (userTasks.Any())
                            {
                                var content = _contentBuilder.BuildContent(user, userTasks);
                                await _emailService.SendEmailAsync(user.Email, "New Task Added", content);
                            }
                        }
                        break;
                    }
                    catch (DbUpdateException ex) when (IsDuplicateReferenceIdException(ex))
                    {
                        attempts++;
                        _logger.LoggWarning("Duplicate reference ID generated — retrying... Attempt {Attempt}", attempts);
                        await Task.Delay(50);
                    }

                    if (attempts == maxAttempts)
                    {
                        throw new Exception("Failed to add task after multiple attempts due to reference ID conflicts.");
                    }
                }
                
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "AddTask - Invalid operation for user ID {UserId}", dto.UserId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LoggError(ex, "AddTask - Database update failed while saving task for user ID {UserId}", dto.UserId);
                throw;
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
        private bool IsDuplicateReferenceIdException(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("Duplicate entry") == true
                && ex.InnerException?.Message.Contains("referenceId") == true;
        }

        public async Task<string> GenerateUniqueNumericIDTaskAsync(string prefix)
        {
            try
            {

                string searchPrefix = prefix + "-";

                var lastTask =await _taskManagementRepository.LastTaskWithPrefix(searchPrefix);

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
                throw;
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
                var task = await _taskManagementRepository.TaskWithIdFindAsync(id);
                if (task == null)
                {
                    throw new NotFoundException($"Task with ID {id} not found.");
                }

                task.taskName = obj.taskName;
                task.UserId = obj.UserId;
                task.dueDate = obj.dueDate;
                task.taskDescription = obj.taskDescription;
                task.taskStatus = obj.taskStatus;
                task.priority = obj.priority;
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

                    var userTasks = await _taskManagementRepository.GetTasksByTaskIdAsync(id);
                    var user = await _userRepository.GetUserByCreatedBy(task.createdBy);

                    if (userTasks.Any())
                    {
                        var content = _contentBuilder.BuildContent(user, userTasks);
                        await _emailService.SendEmailAsync(user.Email, "Task Completed", content);
                    }
                }
            }
           
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred while updating task with ID {TaskId}.", id);
                throw;
            }
        }

        public async Task ProcessFileAsync(int userId, IFormFile file)
        {
            try
            {
                var parser = _parserFactory.GetParser(file.FileName);
                if (parser == null)
                {
                    throw new TaskFileParserException($"No parser found for file: {file.FileName}");
                }

                var rawData = await parser.ParseAsync(file);
                if (rawData == null || !rawData.Any())
                {
                    throw new TaskFileParserException("Parsed data is empty or null.");
                }
                var users = await _userRepository.ListAllUsers();
                var userMap = users.ToDictionary(u => u.UserName.ToLower(), u => u.UserId);

                var tasks = _taskMapper.MapToTasks(rawData, userMap, userId);
                if (tasks == null || !tasks.Any())
                {
                    throw new TaskValidationException("No tasks could be mapped from the file.");
                }

                var tomorrow = DateTime.Today.AddDays(1);
                var validTasks = tasks
                    .Where(t => t.dueDate.Date >= tomorrow)
                    .ToList();
                if (!validTasks.Any())
                {
                    throw new TaskValidationException("Unable to parse the task file due to invalid format.");
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
                    var assigneduserId = entry.Key;
                    var userTasks = entry.Value;

                    var user = await _userRepository.GetUserByIdAsync(assigneduserId);
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
