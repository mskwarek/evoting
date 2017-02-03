﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;

namespace Proxy
{
    class Proxy
    {
        private Configuration configuration;
        private Server server;
        public Server Server
        {
            get { return server; }
        }

        private Client client;
        public Client Client
        {
            get { return client; }
        }

        private List<BigInteger> SRList;
        private Dictionary<string, ProxyBallot> proxyBallots; 
        private int numberOfVoters;
        private Dictionary<BigInteger, List<List<BigInteger>>> serialNumberTokens; 
        public Dictionary<BigInteger, List<List<BigInteger>>> SerialNumberTokens
        {
            get { return this.serialNumberTokens; }
            set { this.serialNumberTokens = value;}
        }
        private Dictionary<BigInteger, BigInteger> serialNumberAndSR;
        private static int numOfSentSLandSR = 0;
        private List<string> yesNoPosition; 

        public Proxy(string configPath)
        {
            this.configuration = new Configuration();
            this.server = new Server(this);
            this.client = new Client(this);
            this.serialNumberTokens = new Dictionary<BigInteger, List<List<BigInteger>>>();
            this.SRList = new List<BigInteger>();
            this.serialNumberAndSR = new Dictionary<BigInteger, BigInteger>();
            this.proxyBallots = new Dictionary<string, ProxyBallot>();

            this.tryToLoadConfig(configPath);
        }

        private void tryToLoadConfig(string configPath)
        {
            try
            {
                this.configuration.loadConfiguration(configPath);
            }
            catch (Exception e)
            {
                Utils.Logs.addLog("PROXY", "UNABLE TO LOAD CONFIGURATION", true, NetworkLib.Constants.LOG_ERROR, true);
            }
        }

        public void connect()
        {
            Utils.Logs.addLog("Proxy", "EA Connecting...", true, NetworkLib.Constants.LOG_INFO);
            bool end = false;
            while (!end)
            {
                try
                {
                    this.Client.connect(configuration.ElectionAuthorityIP, configuration.ElectionAuthorityPort);
                    end = true;
                }
                catch
                {
                    Utils.Logs.addLog("Proxy", "waiting...", true, NetworkLib.Constants.LOG_INFO, true);
                    Thread.Sleep(1000);
                }
            }
            Utils.Logs.addLog("Proxy", "Connected", true, NetworkLib.Constants.LOG_INFO, true);
            Thread.Sleep(5000);
            requestSL();
        }

        private void requestSL()
        {
            string msg = "REQUEST_SL&";

            this.client.sendMessage(msg);
        }

        public void startServer()
        {
            this.Server.startServer(configuration.ProxyPort);
        }

        public void generateSR()
        {
            this.numberOfVoters = this.configuration.NumOfVoters;
            this.SRList = SerialNumberGenerator.generateListOfSerialNumber(this.numberOfVoters, NetworkLib.Constants.NUMBER_OF_BITS_SR);
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.SR_GEN_SUCCESSFULLY, true, NetworkLib.Constants.LOG_INFO);
        }

        public void generateYesNoPosition()
        {
            this.yesNoPosition = new List<string>();
            this.yesNoPosition = SerialNumberGenerator.getYesNoPosition(this.configuration.NumOfVoters, this.configuration.NumOfCandidates);
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.YES_NO_POSITION_GEN_SUCCESSFULL, true, NetworkLib.Constants.LOG_INFO);
            saveYesNoPositionToFile();
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.YES_NO_POSITION_SAVED_TO_FILE, true, NetworkLib.Constants.LOG_INFO);

        }

        public void connectSRandSL()
        {
            for(int i=0; i<this.SRList.Count; i++)
            {
                this.serialNumberAndSR.Add(serialNumberTokens.ElementAt(i).Key, SRList[i]);
            }
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.SR_CONNECTED_WITH_SL, true, NetworkLib.Constants.LOG_INFO, true);
        }

        public void sendSLAndSR(string name)
        {
            if (this.serialNumberAndSR != null && this.serialNumberAndSR.Count != 0)
            {
                BigInteger SL = this.serialNumberAndSR.ElementAt(numOfSentSLandSR).Key;
                BigInteger SR = this.serialNumberAndSR.ElementAt(numOfSentSLandSR).Value;
                List<BigInteger> tokensList = this.serialNumberTokens[SL][0];
                List<BigInteger> exponentesList = this.serialNumberTokens[SL][1];

                this.proxyBallots.Add(name, new ProxyBallot(SL, SR));
                this.proxyBallots[name].TokensList = tokensList;
                this.proxyBallots[name].ExponentsList = exponentesList;


                //there we will save YES-NO positions - previously it was saved when user send 
                //a request but we chaged a idea of our app
                string position = this.yesNoPosition.ElementAt(numOfSentSLandSR);
                this.proxyBallots[name].YesNoPos = position;


                string msg = NetworkLib.Constants.SL_AND_SR + "&" + SL.ToString() + "=" + SR.ToString();
                numOfSentSLandSR += 1;
                this.server.sendMessage(name, msg);
            }
            else
            {
                Utils.Logs.addLog("Proxy", NetworkLib.Constants.ERROR_SEND_SL_AND_SR, true, NetworkLib.Constants.LOG_ERROR, true);
            }
            

        }

        private void saveYesNoPositionToFile()
        {
            try
            {
                string[] yesNoPositionStrTable = this.yesNoPosition.ToArray();
                System.IO.File.WriteAllLines(@"Logs\yesPositions.txt", yesNoPositionStrTable);
                        }
            catch { }
        }

        public void saveVote(string message)
        {
            int[,] vote = new int[this.configuration.NumOfCandidates, 4];
            string[] words = message.Split(';');
            string name = words[0];
            for (int i = 1; i < words.Length-1; i++)
            {
                string[] row = words[i].Split(':'); 
                for (int k = 0; k < row.Length; k++)
                {
                    vote[i-1,k] = Convert.ToInt32(row[k]);
                    Console.WriteLine(vote[i - 1, k]);
                }

            }


            this.proxyBallots[name].Vote = vote;
            this.proxyBallots[name].ConfirmationColumn = Convert.ToInt32(words[words.Length - 1]);
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.VOTE_RECEIVED + name, true, NetworkLib.Constants.LOG_INFO, true);
            this.proxyBallots[name].generateAndSplitBallotMatrix();
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.BALLOT_MATRIX_GEN + name, true, NetworkLib.Constants.LOG_INFO, true);
            BigInteger[] blindProxyBallot = this.proxyBallots[name].prepareDataToSend();
            
            string SL = this.proxyBallots[name].SL.ToString();
            string tokens = prepareTokens(this.proxyBallots[name].SL);
            string columns = prepareBlindProxyBallot(blindProxyBallot);

            string msg = NetworkLib.Constants.BLIND_PROXY_BALLOT + "&" + name + ";"  + SL + ";" + tokens + columns ;

            this.client.sendMessage(msg);
        }

        private string prepareBlindProxyBallot(BigInteger[] blindProxyBallot)
        {
            string columns = "";
            for (int i = 0; i < blindProxyBallot.Length-1; i++)
            {
                columns = columns + blindProxyBallot[i].ToString() + ",";    
            }
            columns += blindProxyBallot[blindProxyBallot.Length - 1].ToString();

            return columns;
        }

        private string listToMessage(List<BigInteger> tokenList)
        {
            string tokens = null;
            for (int i = 0; i < tokenList.Count - 1; i++)
            {
                tokens = tokens + tokenList[i].ToString() + ",";
            }
            return (tokens + tokenList[tokenList.Count - 1].ToString() + ";");
        }

        private string prepareTokens(BigInteger SL)
        {
            List<BigInteger> tokenList = this.serialNumberTokens[SL][0];
            string tokens = listToMessage(tokenList);

            List<BigInteger> exponentsList = this.serialNumberTokens[SL][1];
            tokens += listToMessage(exponentsList);

            return tokens;
        }

        public void saveSignedBallot(string message)
        {
            string[] words = message.Split(';');

            foreach (string s in words)
            {
                Console.WriteLine(s);
            }


            string name = words[0];

            string[] signedColumns = words[1].Split(',');
            List<BigInteger> signedColumnsList = new List<BigInteger>();
            foreach (string s in signedColumns)
            {
                BigInteger big = new BigInteger(s);
                signedColumnsList.Add(big);
            }

            this.proxyBallots[name].SignedColumns = signedColumnsList;
            Utils.Logs.addLog("Proxy", NetworkLib.Constants.SIGNED_COLUMNS_RECEIVED, true, NetworkLib.Constants.LOG_INFO, true);

            this.sendSignedColumnToVoter(name);
            this.unblindSignedBallotMatrix(name);
        }

        private void sendSignedColumnToVoter(string name)
        {
            int confirmation = this.proxyBallots[name].ConfirmationColumn;
            string token = this.proxyBallots[name].TokensList[confirmation].ToString(); 

            BigInteger signedBlindColumn = this.proxyBallots[name].SignedColumns[confirmation];
            string signedBlindColumnStr = signedBlindColumn.ToString();
            string message = NetworkLib.Constants.SIGNED_COLUMNS_TOKEN + "&" + signedBlindColumnStr + ";" + token;
            this.server.sendMessage(name, message);
        }

        private void unblindSignedBallotMatrix(string name)
        {

            BigInteger[]  signedColumns = this.proxyBallots[name].SignedColumns.ToArray();
            string[] strUnblindedBallotMatrix = this.proxyBallots[name].unblindSignedData(signedColumns);
            Console.WriteLine("odslepiona ballotMatrix");
            string unblinedColumns = null;
            for (int i =0; i<strUnblindedBallotMatrix.Length;i++)
            {
                Console.WriteLine(strUnblindedBallotMatrix[i]);
                unblinedColumns = unblinedColumns + strUnblindedBallotMatrix[i] + ","; 
            }
            unblinedColumns += strUnblindedBallotMatrix[strUnblindedBallotMatrix.Length - 1];


            string message = NetworkLib.Constants.UNBLINED_BALLOT_MATRIX + "&" + name + ";" + unblinedColumns;

            this.client.sendMessage(message);
        }
    }
}
