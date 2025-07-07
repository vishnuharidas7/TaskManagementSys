using System.Net.Http;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{
    public class TaskFileParserFactory : ITaskFileParserFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TaskFileParserFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "serviceProvider cannot be null.");
        }

        public ITaskFileParser GetParser(string fileName)
        {
            try
            {
                if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return _serviceProvider.GetRequiredService<ExcelTaskFileParser>();
                else if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    return _serviceProvider.GetRequiredService<CsvTaskFileParser>();

                throw new NotSupportedException("Unsupported file format.");
            }
            catch (InvalidOperationException ex)
            {
                
                throw;
            }
        }
    }
}
