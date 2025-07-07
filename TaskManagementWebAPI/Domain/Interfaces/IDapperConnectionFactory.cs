using System.Data;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IDapperConnectionFactory
    {
        /// <summary>
        /// For creating a connection with DataBase
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();
    }
}
