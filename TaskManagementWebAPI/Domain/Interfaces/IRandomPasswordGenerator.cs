namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IRandomPasswordGenerator
    {
        /// <summary>
        /// For generating random password
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        string GenerateRandomPassword(int length);
    }
}
