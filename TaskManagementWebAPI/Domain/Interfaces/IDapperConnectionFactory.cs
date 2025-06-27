using System.Data;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IDapperConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
