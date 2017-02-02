using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ElectionAuthority
{
    class CandidateList
    {
        private List<String> candidateDefaultList;

        public CandidateList()
        {
            candidateDefaultList = new List<String>();

        }

        public int getNumberOfCandidates()
        {
            return this.candidateDefaultList.Count;
        }
        public List<string> loadCanidateList(string path)
        {
            List<string> candidate = new List<string>();
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(path);

                string nodeName = "//Candidates/Candidate";
                foreach (XmlNode xnode in xml.SelectNodes(nodeName))
                {
                    string input = xnode.Attributes[NetworkLib.Constants.ID].Value;
                    candidate.Add(input);
                }
                Utils.Logs.addLog("EA", NetworkLib.Constants.CANDIDATE_LIST_SUCCESSFUL, true, NetworkLib.Constants.LOG_INFO, true);              
            }
            catch (Exception)
            {
                Console.WriteLine("Wyjatek w loadCandidateList");
            }
            this.candidateDefaultList = candidate;
        }

        public string getCandidateByIndex(int index)
        {
            return this.candidateDefaultList[index];
        }

        public string getCandidateListPermuated(string name, List<BigInteger> permutation)
        {
            List<String> candidateList = new List<string>();

            for (int i = 0; i < this.candidateDefaultList.Count; i++)
            {
                int index = permutation[i].IntValue;
                candidateList.Add(candidateDefaultList[index - 1]);
            }

            string candidateListString = NetworkLib.Constants.CANDIDATE_LIST_RESPONSE + "&";
            for (int i = 0; i < candidateList.Count - 1; i++)
            {
                candidateListString += candidateList[i] + ";";
            }
            candidateListString += candidateList[candidateList.Count - 1];

            return candidateListString;
        }
    }
}
