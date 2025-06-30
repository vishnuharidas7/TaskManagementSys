using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.PasswordService
{
    public class RandomPasswordGenerator: IRandomPasswordGenerator
    {
        private IAppLogger<RandomPasswordGenerator> _logger;
        public RandomPasswordGenerator(IAppLogger<RandomPasswordGenerator> logger)
        {
            _logger=logger;
        }
        private const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        public string GenerateRandomPassword(int length)
        {
            try
            {
                var random = new Random();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("ViewUser for password resert - View user failed");
                throw;
            }
        }


    }
}
