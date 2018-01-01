using System;
using Common;

namespace voter
{
    public class Configuration : IConfiguration
    {
        IFileHelper fileHelper;
        public Configuration(IFileHelper fileHelper)
        {
            this.fileHelper = fileHelper;
        }

        public T readConfiguration<T>(string path)
            where T : new()
        {
            return JsonApplicationConfig.Load<T>(fileHelper, path);
        }
    }

    public class ConfigurationJson
    {
        public String eaIp;
        public String eaPort;
        public String proxyIp;
        public String proxyPort;
        public int numberOfCandidates;
        public String name;
    }
}