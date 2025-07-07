using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService
{
    public class TaskStatusRepository : ITaskStatusRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppLogger<TaskStatusRepository> _logger;

        public TaskStatusRepository(ApplicationDbContext context,IAppLogger<TaskStatusRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "context cannot be null.");
            _logger= logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
        }

        public IEnumerable<Tasks> GetAllTasks()
        {
            try
            {
                return _context.Task.ToList();
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Failed to retrieve tasks from database.");
                throw;
            }

        }

        public void SaveAllTasks()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Failed to retrieve tasks from database.");
                throw;
            }
        }
    }
}
