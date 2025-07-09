using Microsoft.AspNetCore.Connections;
using MySqlConnector;
using System.Data;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;

namespace TaskManagementWebAPI.Application.Services.FileUpload
{
    public class DapperConnectionFactory : IDapperConnectionFactory
    {
        private readonly string _connectionString;

        public DapperConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "configuration cannot be null.");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
