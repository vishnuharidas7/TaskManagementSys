using Microsoft.EntityFrameworkCore;
using System;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Application.Services
{
    public class InMemoryTaskRepository : ITaskEmailRepository
    {
        private readonly ApplicationDbContext _context;

        public InMemoryTaskRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null.");
        }

        public IEnumerable<Tasks> GetTasksByUserId(int userId)
        {
            try
            {
                return _context.Task
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
