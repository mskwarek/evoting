using Common;

namespace voter
{
    public class Voter : IVoter
    {
        public ConfigurationJson voterConfig { get; private set; }
        public Voter()
        { }   

        public void readConfiguration(IFileHelper fileHelper, string path)
        {
            voterConfig = new Configuration(fileHelper).readConfiguration<ConfigurationJson>(path);
        }

    }
}