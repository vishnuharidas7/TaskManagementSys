using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class TaskStatusUpdateServiceRepository : ITaskStatusUpdateServiceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppLogger<TaskStatusUpdateServiceRepository> _logger;

        public TaskStatusUpdateServiceRepository(ApplicationDbContext context,IAppLogger<TaskStatusUpdateServiceRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger= logger ?? throw new ArgumentNullException(nameof(logger));
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
