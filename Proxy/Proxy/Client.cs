using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using NetworkLib;
using Newtonsoft.Json;
using Org.BouncyCastle.Math;

namespace Proxy
{
    class Client
    {
        private NetworkLib.Client client;
        public NetworkLib.Client.NewMsgHandler newMessageHandler { get; set; }

        public Client(Proxy proxy)
        {
        }

        public bool connect(string ip, string port)
        {
            client = new NetworkLib.Client(ip, port);
            newMessageHandler = new NetworkLib.Client.NewMsgHandler(displayMessageReceived);
            client.OnNewMessageRecived += newMessageHandler;
            sendMyName();
            return true;
        }

        private void sendMyName()
        {
            client.sendMessage("//NAME// " + "PROXY");
        }




        private void parseSLTokensDictionaryFromEA(string msg)
        {
            Dictionary<BigInteger, List<List<BigInteger>>> dict = new Dictionary<BigInteger, List<List<BigInteger>>>();
            MessageSLTokens msgData = JsonConvert.DeserializeObject<MessageSLTokens>(msg);


            string[] dictionaryElem = msg.Split(';');
            for (int i = 0; i < dictionaryElem.Length - 1; i++)
            {
                string[] words = dictionaryElem[i].Split('=');
                BigInteger SL = new BigInteger(words[0]);
                List<List<BigInteger>> mainList = new List<List<BigInteger>>();

                string[] token = words[1].Split(':');

                string[] tokenList = token[0].Split(',');
                List<BigInteger> firstList = new List<BigInteger>();
                foreach (string str in tokenList)
                {
                    firstList.Add(new BigInteger(str));
                }
                mainList.Add(firstList);

                string[] exponentsList = token[1].Split(',');
                List<BigInteger> secondList = new List<BigInteger>();
                foreach (string str in exponentsList)
                {
                    secondList.Add(new BigInteger(str));
                }
                mainList.Add(secondList);

                dict.Add(SL, mainList);
            }

            this.proxy.SerialNumberTokens = dict;
            this.proxy.connectSRandSL();
            this.sendMessage(NetworkLib.Constants.SL_RECEIVED_SUCCESSFULLY + "&");

        }

        private void displayMessageReceived(object myObject, MessageArgs myArgs)
        {
            Utils.Logs.addLog("Client", Constants.NEW_MSG_RECIVED + " " + myArgs.Message, true, Constants.LOG_INFO, true);

            NetworkLib.JsonMessage jsonmsg = JsonConvert.DeserializeObject<NetworkLib.JsonMessage>(myArgs.Message);

            switch (jsonmsg.header)
            {
                case NetworkLib.Constants.SL_TOKENS:
                    parseSLTokensDictionaryFromEA(jsonmsg.data);
                    Utils.Logs.addLog("Client", NetworkLib.Constants.SL_RECEIVED, true, NetworkLib.Constants.LOG_INFO, true);
                    break;
                case NetworkLib.Constants.CONNECTED:
                    Utils.Logs.addLog("Client", NetworkLib.Constants.PROXY_CONNECTED_TO_EA, true, NetworkLib.Constants.LOG_INFO, true);
                    break;
                case NetworkLib.Constants.SIGNED_PROXY_BALLOT:
                    this.proxy.saveSignedBallot(jsonmsg.data);
                    break;
            }       
        }

        public void disconnectFromElectionAuthority(bool error = false)
        {
            client.stopService();
        }

        public void sendMessage(string msg)
        {
            client.sendMessage(msg);
        }
    }
}
