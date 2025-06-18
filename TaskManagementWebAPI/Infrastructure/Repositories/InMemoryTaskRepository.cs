using System;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class InMemoryTaskRepository : ITaskEmailRepository
    {
        private readonly ApplicationDbContext _context;

        public InMemoryTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Tasks> GetTasksByUserId(int userId)
        {
            return _context.Task
                .Where(t => t.UserId == userId)
                .ToList();
        }
    }
}
