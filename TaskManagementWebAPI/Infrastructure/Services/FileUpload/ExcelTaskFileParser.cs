using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{

        public class ExcelTaskFileParser : ITaskFileParser
        {
        //public async Task<List<Tasks>> ParseAsync(IFormFile file)
        //{
        //    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        //        throw new InvalidDataException("Only .xlsx files are supported.");

        //    var tasks = new List<Tasks>();

        //    using var stream = new MemoryStream();
        //    await file.CopyToAsync(stream);
        //    stream.Position = 0;

        //    IWorkbook workbook = new XSSFWorkbook(stream);
        //    ISheet sheet = workbook.GetSheetAt(0);

        //    if (sheet == null || sheet.LastRowNum < 1)
        //        return tasks;

        //    for (int row = 1; row <= sheet.LastRowNum; row++)
        //    {
        //        IRow currentRow = sheet.GetRow(row);
        //        if (currentRow == null) continue;

        //        var task = new Tasks
        //        {
        //            taskName = currentRow.GetCell(0)?.ToString(),
        //            UserId = int.TryParse(currentRow.GetCell(1)?.ToString(), out var uid) ? uid : 0,
        //            dueDate = DateTime.TryParse(currentRow.GetCell(2)?.ToString(), out var dt) ? dt : DateTime.MinValue,
        //            taskDescription = currentRow.GetCell(3)?.ToString(),
        //            priority = currentRow.GetCell(4)?.ToString()
        //        };

        //        tasks.Add(task);
        //    }

        //    return tasks;
        //}

        public async Task<List<Dictionary<string, object>>> ParseAsync(IFormFile file)
        {
            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Only .xlsx files are supported.");

            var data = new List<Dictionary<string, object>>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);

            if (sheet == null || sheet.LastRowNum < 1)
                return data;

            var headerRow = sheet.GetRow(0);
            if (headerRow == null) return data;

            var headers = new List<string>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                headers.Add(headerRow.GetCell(i)?.ToString() ?? $"Column{i}");
            }

            for (int row = 1; row <= sheet.LastRowNum; row++)
            {
                IRow currentRow = sheet.GetRow(row);
                if (currentRow == null) continue;

                var rowDict = new Dictionary<string, object>();
                for (int col = 0; col < headers.Count; col++)
                {
                    var cellValue = currentRow.GetCell(col)?.ToString();
                    rowDict[headers[col]] = cellValue;
                }

                data.Add(rowDict);
            }

            return data;
        }

    }

}
