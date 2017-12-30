namespace voter
{
    public interface IConfiguration
    {
          T readConfiguration<T>(string path) where T : new();
    }
}