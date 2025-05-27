using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Xml.Linq;
using TaskManagement_Project.DTOs;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Models;


namespace TaskManagementWebAPI.Repositories
{
    public class AssignUserRepository : IAssignUserRepository
    {
        private readonly ApplicationDbContext _db;

        public AssignUserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<AssignUserDTO>> ViewUsers()
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

        public async Task<List<ViewTasksDTO>> viewAllTasks()
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
                taskStatus = u.taskStatus
            })
            .ToListAsync();

            return viewAlltasks;
        }

        public async Task AddTask(AddTaskDTO dto)
        { 
            var task = new Tasks
            {
               taskName = dto.taskName,
               taskDescription = dto.taskDescription,
               UserId = dto.UserId,
               dueDate = dto.dueDate,
               //taskStatus = dto.taskStatus
            };

            _db.Task.Add(task);
            await _db.SaveChangesAsync();

            // return user;
        }

        //public async Task<List<AddTaskDTO>> ProcessExcelFileAsync(IFormFile file)
        //{

        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    var tasks = new List<AddTaskDTO>();

        //    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        //        throw new InvalidDataException("Only .xlsx files are supported.");

        //    using (var stream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(stream);



        //        using var package = new ExcelPackage(stream);

        //            if (package.Workbook.Worksheets.Count == 0)
        //            {
        //                throw new InvalidOperationException("The Excel file does not contain any worksheets.");
        //            }

        //            var worksheet = package.Workbook.Worksheets[0];
        //            if (worksheet == null) return tasks;

        //            int rowCount = worksheet.Dimension.Rows;
        //            for (int row = 2; row <= rowCount; row++)
        //            {
        //                tasks.Add(new AddTaskDTO
        //                {
        //                    taskName = worksheet.Cells[row, 1].Text,
        //                    UserId = int.TryParse(worksheet.Cells[row, 2].Text, out var uid) ? uid : 0,
        //                    dueDate = DateTime.TryParse(worksheet.Cells[row, 3].Text, out var date) ? date : DateTime.MinValue,
        //                    description = worksheet.Cells[row, 4].Text
        //                });
        //            }

        //    }

        //    return tasks;
        //}

        //public async Task<List<AddTaskDTO>> ProcessExcelFileAsync(IFormFile file)
        //{
        //    //ExcelPackage.License = LicenseContext.NonCommercial;  // EPPlus 8+ way
        //    //ExcelPackage.SetLicense(LicenseContext.NonCommercial);
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    var tasks = new List<AddTaskDTO>();

        //    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        //        throw new InvalidDataException("Only .xlsx files are supported.");

        //    using (var stream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(stream);
        //        stream.Position = 0;  // Reset position BEFORE reading

        //        using var package = new ExcelPackage(stream);

        //        if (package.Workbook.Worksheets.Count == 0)
        //            throw new InvalidOperationException("The Excel file does not contain any worksheets.");

        //        var worksheet = package.Workbook.Worksheets[0];
        //        if (worksheet == null || worksheet.Dimension == null)
        //            return tasks;  // No data

        //        int rowCount = worksheet.Dimension.Rows;

        //        for (int row = 2; row <= rowCount; row++)  // Assuming first row is header
        //        {
        //            tasks.Add(new AddTaskDTO
        //            {
        //                taskName = worksheet.Cells[row, 1].Text,
        //                UserId = int.TryParse(worksheet.Cells[row, 2].Text, out var uid) ? uid : 0,
        //                dueDate = DateTime.TryParse(worksheet.Cells[row, 3].Text, out var date) ? date : DateTime.MinValue,
        //                description = worksheet.Cells[row, 4].Text
        //            });
        //        }
        //    }

        //    return tasks;
        //}

        //public async Task<List<AddTaskDTO>> ProcessExcelFileAsync(IFormFile file)
        //{
        //    var tasks = new List<AddTaskDTO>();

        //    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        //        throw new InvalidDataException("Only .xlsx files are supported.");

        //    using var stream = new MemoryStream();
        //    {
        //        await file.CopyToAsync(stream);

        //    }

        //    using (var reader = new StreamReader(stream))
        //    {
        //        do
        //        {
        //            bool isHeaderSkipped = false;
        //            while (reader.Read())
        //            {
        //                if (!isHeaderSkipped)
        //                {
        //                    isHeaderSkipped = true;
        //                    continue;
        //                }
        //                AddTaskDTO s = new AddTaskDTO();
        //                s.taskName = reader.GetValue(1).ToString();
        //                s.UserId = Convert.ToInt32(reader.GetValue(2));
        //                s.dueDate = Convert.ToDateTime(reader.GetValue(3).ToString());
        //                s.description = reader.GetValue(4).ToString();
        //                _db.Task.Add(s);
        //                await _db.SaveChangesAsync();
        //            }
        //        } while (reader.NextResult());
        //    }
        //}

        //public async Task<List<AddTaskDTO>> ProcessExcelFileAsync(IFormFile file)
        //{
        //    var tasks = new List<AddTaskDTO>();

        //    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        //        throw new InvalidDataException("Only .xlsx files are supported.");

        //    using var stream = new MemoryStream();
        //    await file.CopyToAsync(stream);
        //    stream.Position = 0;

        //    IWorkbook workbook = new XSSFWorkbook(stream);
        //    ISheet sheet = workbook.GetSheetAt(0); // Read the first worksheet

        //    if (sheet == null || sheet.LastRowNum < 1)
        //        return tasks;

        //    for (int row = 1; row <= sheet.LastRowNum; row++) // Skip header row at index 0
        //    {
        //        IRow currentRow = sheet.GetRow(row);
        //        if (currentRow == null) continue;

        //        var task = new AddTaskDTO
        //        {
        //            taskName = currentRow.GetCell(0)?.ToString(),
        //            UserId = int.TryParse(currentRow.GetCell(1)?.ToString(), out var uid) ? uid : 0,
        //            dueDate = DateTime.TryParse(currentRow.GetCell(2)?.ToString(), out var dt) ? dt : DateTime.MinValue,
        //            description = currentRow.GetCell(3)?.ToString()
        //        };

        //       // tasks.Add(task);
        //        _db.Task.Add(task); // Assuming _db is your EF DbContext
        //    }

        //    await _db.SaveChangesAsync();
        //    //return tasks;
        //}

        public async Task ProcessExcelFileAsync(IFormFile file)
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
                    taskDescription = currentRow.GetCell(3)?.ToString()
                };

                _db.Task.Add(taskEntity); // Or _db.Tasks.Add(...) based on your DbSet name
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteTask(int id)
        {
            var task = await _db.Task.FindAsync(id);
            if (task == null)
            {
                throw new Exception("Task not found");
            }
            _db.Task.Remove(task);
            await _db.SaveChangesAsync();

        }

        public async Task UpdateTask(int id, AddTaskDTO obj)
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
      
            await _db.SaveChangesAsync();
        }

    }
}
