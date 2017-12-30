using Common;

namespace voter
{
    public class Voter : IVoter
    {
        Configuration configuration;
        public Voter(IFileHelper fileHelper)
        {
            configuration = new Configuration(fileHelper);
        }   

        public void readConfiguration(string path)
        {
            configuration.readConfiguration<ConfigurationJson>(path);
        }

    }
}