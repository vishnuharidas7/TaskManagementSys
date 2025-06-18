using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{
    public class TaskFileParserFactory : ITaskFileParserFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TaskFileParserFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITaskFileParser GetParser(string fileName)
        {
            if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return _serviceProvider.GetRequiredService<ExcelTaskFileParser>();
            else if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return _serviceProvider.GetRequiredService<CsvTaskFileParser>();

            throw new NotSupportedException("Unsupported file format.");
        }
    }
}
