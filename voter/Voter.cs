using Common;
using Newtonsoft.Json;

namespace voter
{
    public class Voter : IVoter
    {
        public ConfigurationJson voterConfig { get; private set; }
        private VoterCrypto voterCrypto;
        private IClient proxyTransportLayer;
        private IClient electionAuthTransportLayer;
        public Voter(IClient proxyTransportLayer, IClient electionAuthTransportLayer)
        {
            this.proxyTransportLayer = proxyTransportLayer;
            this.electionAuthTransportLayer = electionAuthTransportLayer;
            voterConfig = new ConfigurationJson();
            voterCrypto = new VoterCrypto();
        }   

        public void readConfiguration(IFileHelper fileHelper, string path)
        {
            voterConfig = new Configuration(fileHelper).readConfiguration<ConfigurationJson>(path);
        }

        public void requestForSrAndSlFromProxy()
        {
            this.proxyTransportLayer.sendMessage(buildSrAndSlReq());
        }

        private string buildSrAndSlReq()
        {
            var request = new Common.Messages.SlSrReq();
            request.senderName = this.voterConfig.name;

            return JsonConvert.SerializeObject(request);
        }

        public void requestForCandidateListFromElectionAuth()
        {
            this.electionAuthTransportLayer.sendMessage(buildCandidateListReq());
        }

        private string buildCandidateListReq()
        {
            var request = new Common.Messages.CandidateListReq();
            request.senderName = this.voterConfig.name;
            request.slValue = this.voterCrypto.SL.ToString();

            return JsonConvert.SerializeObject(request);
        }

    }
}