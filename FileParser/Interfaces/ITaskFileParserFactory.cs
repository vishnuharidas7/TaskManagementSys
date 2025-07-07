namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskFileParserFactory
    {
        /// <summary>
        /// To get which file type to be used for file extraction
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        ITaskFileParser GetParser(string fileName);
    }
}
