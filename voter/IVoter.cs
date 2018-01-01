using Common;

namespace voter
{
    public interface IVoter
    {
        void readConfiguration(IFileHelper fileHelper, string path);
    }
}