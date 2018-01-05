using Common;
using Newtonsoft.Json;

namespace voter
{
    public class Voter : IVoter
    {
        public ConfigurationJson voterConfig { get; private set; }
        private IClient proxyTransportLayer;
        public Voter(IClient proxyTransportLayer)
        {
            this.proxyTransportLayer = proxyTransportLayer;
            voterConfig = new ConfigurationJson();
        }   

        public void readConfiguration(IFileHelper fileHelper, string path)
        {
            voterConfig = new Configuration(fileHelper).readConfiguration<ConfigurationJson>(path);
        }

        public void requestForSrAndSl()
        {
            this.proxyTransportLayer.sendMessage(buildSrAndSlReq());
        }

        private string buildSrAndSlReq()
        {
            var request = new Common.Messages.SlSrReq();
            request.senderName = this.voterConfig.name;
            Common.Logger.log(JsonConvert.SerializeObject(request));
            return JsonConvert.SerializeObject(request);
        }

    }
}