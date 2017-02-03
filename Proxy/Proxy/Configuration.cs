using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Proxy
{
    class Configuration
    {
        private XmlDocument xml;

        string id = "ID";
        string port = "Port";
        string ea_ip = "EA_IP";
        string ea_port = "EA_Port";
        string voters_cnt = "VotersCnt";
        string candidates_cnt = "CandidatesCnt";

        Dictionary<string, string> attributes;

        private string proxyID;
        public string ProxyID
        {
            get { return proxyID; }
        }

        private string proxyPort;
        public string ProxyPort
        {
            get { return proxyPort; }
        }

        private string electionAuthorityIP;
        public string ElectionAuthorityIP
        {
            get { return electionAuthorityIP; }
        }

        private string electionAuthorityPort;
        public string ElectionAuthorityPort
        {
            get { return electionAuthorityPort; }
        }

        private int numOfVoters;
        public int NumOfVoters
        {
            get { return numOfVoters; }
        }

        private int numOfCandidates;
        public int NumOfCandidates
        {
            get { return numOfCandidates; }
        }

        public Configuration()
        {
            this.xml = new XmlDocument();
            this.attributes = new Dictionary<string, string>{
                    { this.id, NetworkLib.Constants.ID },
                    { this.port, NetworkLib.Constants.PROXY_PORT},
                    { this.ea_ip, NetworkLib.Constants.ELECTION_AUTHORITY_IP },
                    { this.ea_port, NetworkLib.Constants.ELECTION_AUTHORITY_PORT },
                    { this.voters_cnt, NetworkLib.Constants.NUMBER_OF_VOTERS },
                    { this.candidates_cnt, NetworkLib.Constants.NUMBER_OF_CANDIDATES }
             };
        }

        private String readConfig(XmlDocument xml, string attribute)
        {
            foreach (XmlNode xnode in xml.SelectNodes("//Proxy[@ID]"))
            {
                return xnode.Attributes[attribute].Value;
            }
            return "";
        }

        private string getAttribute()
        {
            
            return "";
        }

        public void loadConfiguration(string path)
        {
            xml.Load(path);

            this.proxyID = readConfig(xml, attributes[id]);
            this.proxyPort = readConfig(xml, attributes[port]);
            this.electionAuthorityIP = readConfig(xml, attributes[this.ea_ip]);
            this.electionAuthorityPort = readConfig(xml, attributes[ea_port]);
            this.numOfVoters = Convert.ToInt32(readConfig(xml, attributes[this.voters_cnt]));
            this.numOfCandidates = Convert.ToInt32(readConfig(xml, attributes[this.candidates_cnt]));

            string[] filePath = path.Split('\\');
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.CONFIGURATION_LOADED_FROM + filePath[filePath.Length - 1], true, NetworkLib.Constants.LOG_INFO);
        }
    }
}
