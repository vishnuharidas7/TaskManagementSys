using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Application.Services
{
    public class InMemoryUserRepository : IUserEmailRepository
    {
        private readonly ApplicationDbContext _context;

        public InMemoryUserRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null.");
        }

        public IEnumerable<Users> GetAllUsers()
        {
            try
            {
                return _context.User.ToList();
            }
            catch (ArgumentNullException argEx)
            {
                throw;
            }
            catch (InvalidOperationException invEx)
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
        public Users GetUserById(int id)
        {
            try
            {
                var user = _context.User.Find(id);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {id} not found.");

                return user;
            }
            catch (ArgumentNullException argEx)
            {
                throw;
            }
            catch (InvalidOperationException invEx)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw;
            }
            catch (KeyNotFoundException)
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
