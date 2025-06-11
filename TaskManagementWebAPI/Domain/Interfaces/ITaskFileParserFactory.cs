namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskFileParserFactory
    {
        ITaskFileParser GetParser(string fileName);
    }
}
