namespace Common
{
    public interface IFileHelper
    {
        bool DirectoryExists(string location);
        string ReadAllText(string location);
        void WriteAllText(string location, string text);
    }
}