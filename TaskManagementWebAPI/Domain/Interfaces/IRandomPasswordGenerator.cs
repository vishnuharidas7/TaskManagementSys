namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IRandomPasswordGenerator
    {
        string GenerateRandomPassword(int length);
    }
}
