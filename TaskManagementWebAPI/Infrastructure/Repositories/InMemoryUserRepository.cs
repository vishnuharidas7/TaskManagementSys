using System;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class InMemoryUserRepository : IUserEmailRepository
    {
        private readonly ApplicationDbContext _context;

        public InMemoryUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Users> GetAllUsers() => _context.User.ToList();

        public Users GetUserById(int id) => _context.User.Find(id);
    }
}
