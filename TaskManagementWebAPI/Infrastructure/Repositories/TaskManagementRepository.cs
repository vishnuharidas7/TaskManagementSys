using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Xml.Linq; 
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 
using TaskManagementWebAPI.Infrastructure.Persistence;


namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class TaskManagementRepository : ITaskManagementRepository
    {
        private readonly ApplicationDbContext _db;

        public TaskManagementRepository(ApplicationDbContext db)
        {
            _db = db;
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
                    priority = u.priority
                })
                .ToListAsync();

                return viewAlltasks;
            }
            catch(Exception ex)
            {
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
                   priority = dto.priority
                   //taskStatus = dto.taskStatus
                };

                _db.Task.Add(task);
                await _db.SaveChangesAsync();

                    // return user;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        
        public async Task ProcessExcelFileAsync(IFormFile file)
        {
            try
            {
                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Only .xlsx files are supported.");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                IWorkbook workbook = new XSSFWorkbook(stream);
                ISheet sheet = workbook.GetSheetAt(0); // First sheet

                if (sheet == null || sheet.LastRowNum < 1)
                    return;

                for (int row = 1; row <= sheet.LastRowNum; row++) // Row 0 = header
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow == null) continue;

                    var taskEntity = new Tasks
                    {
                        taskName = currentRow.GetCell(0)?.ToString(),
                        UserId = int.TryParse(currentRow.GetCell(1)?.ToString(), out var uid) ? uid : 0,
                        dueDate = DateTime.TryParse(currentRow.GetCell(2)?.ToString(), out var dt) ? dt : DateTime.MinValue,
                        taskDescription = currentRow.GetCell(3)?.ToString(),
                        priority = currentRow.GetCell(4)?.ToString()
                    };

                    _db.Task.Add(taskEntity); // Or _db.Tasks.Add(...) based on your DbSet name
                }

                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
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

                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
        }
 
        public async Task<IEnumerable<AddTaskDTO>> GetTasksByUserId(int userId)
        {
            try
            {
                var tasks = await _db.Task
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId)
                    .Select(t => new AddTaskDTO
                    {
                        taskId = t.taskId,
                        taskName = t.taskName,
                        taskDescription = t.taskDescription,
                        taskStatus = t.taskStatus,
                        dueDate = t.dueDate,
                        priority = t.priority,
                        UserId = t.UserId,
                        userName = t.User.UserName
                    })
                    .ToListAsync();

                return tasks;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

    }
}
