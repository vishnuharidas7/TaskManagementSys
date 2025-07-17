using LoggingLibrary.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Services.FileUpload;

namespace FileParser.Factory
{
    public class TaskFileParserFactory : ITaskFileParserFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger<TaskFileParserFactory> _logger;
        public TaskFileParserFactory(IServiceProvider serviceProvider, IAppLogger<TaskFileParserFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "serviceProvider cannot be null.");
            _logger = logger;
        }

        public ITaskFileParser GetParser(string fileName)
        {
            try
            {
                _logger.LoggInformation("ITaskFileParser started.");
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
