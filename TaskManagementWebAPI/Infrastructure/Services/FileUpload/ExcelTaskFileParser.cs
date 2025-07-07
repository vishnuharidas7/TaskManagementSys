using LoggingLibrary.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{

        public class ExcelTaskFileParser : ITaskFileParser
        {
        public async Task<List<Dictionary<string, object>>> ParseAsync(IFormFile file)
        {
            try
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
            catch (InvalidDataException ex)
            {
              
                throw;
            }
            catch (IOException ex)
            {
              
                throw;
            }
            //catch (OfficeOpenXml.Packaging.InvalidDataException ex)
            //{
            //    throw new InvalidOperationException("The Excel file is corrupted or unreadable.", ex);
            //}
            catch (Exception ex)
            {
               
                throw;
            }

        }

    }

}
