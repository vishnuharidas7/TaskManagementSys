using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService
{
    public class TaskStatusRepository : ITaskStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Tasks> GetAllTasks()
        {
            return _context.Task.ToList(); 
        }

        public void SaveAllTasks()
        {
            _context.SaveChanges();
        }
    }
}
