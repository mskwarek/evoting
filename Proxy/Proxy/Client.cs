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

        public delegate void SLMessageReceived(List<MessageSLTokens> list);
        public event SLMessageReceived OnNewSLMessageReceived;

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
            List<MessageSLTokens> msgData = JsonConvert.DeserializeObject<List<MessageSLTokens>>(msg);
            this.OnNewSLMessageReceived(msgData);            
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
                    //this.proxy.saveSignedBallot(jsonmsg.data);
                    break;
            }       
        }

        public void disconnectFromElectionAuthority(bool error = false)
        {
            client.stopService();
        }

        public void sendMessage(string header, string msg)
        {
            JsonMessage json = new JsonMessage(header, msg);
            string message = JsonConvert.SerializeObject(json);
            client.sendMessage(message);
        }
    }
}
