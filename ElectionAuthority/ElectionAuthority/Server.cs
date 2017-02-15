using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Newtonsoft.Json;

namespace ElectionAuthority
{
    public class Server
    {
        private Dictionary<TcpClient, string> clientSockets;
        private Parser parser;

        NetworkLib.Server server;

        public Server(ElectionAuthority electionAuthority)
        {
            clientSockets = new Dictionary<TcpClient, string>();
            this.parser = new Parser(electionAuthority);
        }

        public bool startServer(string port)
        {
            int runningPort = Convert.ToInt32(port);
            this.server = new NetworkLib.Server(runningPort);
            Utils.Logs.addLog("EA", NetworkLib.Constants.SERVER_STARTED_CORRECTLY, true, NetworkLib.Constants.LOG_INFO, true);
            server.OnNewClientRequest += this.newClientConnected;
            server.OnNewMessageRecived += this.displayMessageReceived;
            return true;
        }

        private void displayMessageReceived(object myObject, NetworkLib.MessageArgs myArgs)
        {
            Utils.Logs.addLog("EA MSG RECEIVED", myArgs.Message, true, NetworkLib.Constants.LOG_MESSAGE, true); //do usuniecia ale narazie widzim co leci w komuniakcji

            if (!clientSockets.ContainsKey(myArgs.ID))
            {
                updateClientName(myArgs.ID, myArgs.Message);
                sendMessage(clientSockets[myArgs.ID], NetworkLib.Constants.CONNECTED, "");
            }
            this.parser.parseMessage(myArgs.Message);      
        }

        private void newClientConnected(object myObject, NetworkLib.ClientArgs myArgs)
        {
            Utils.Logs.addLog("EA NEW NODE", NetworkLib.Constants.NEW_MSG_RECIVED + " " + myArgs.NodeName, true, NetworkLib.Constants.LOG_INFO, true);
        }

        public void stopServer()
        {
            this.server.stopServer();
        }

        public void sendMessage(string name, string header, string msg)
        {
            TcpClient client = null;
            List<TcpClient> clientsList = clientSockets.Keys.ToList();
            for (int i = 0; i < clientsList.Count; i++)
            {
                if (clientSockets[clientsList[i]].Equals(name))
                {
                    client = clientsList[i];
                    break;
                }
            }
            NetworkLib.JsonMessage request = new NetworkLib.JsonMessage(header, msg);
            string json = JsonConvert.SerializeObject(request);

            this.server.sendMessage(client, json);
        }

        private void updateClientName(TcpClient client, string signal)
        {
            if (signal.Contains("//NAME// "))
            {
                string[] tmp = signal.Split(' ');
                clientSockets[client] = tmp[1];
            }
        }
    }
}
