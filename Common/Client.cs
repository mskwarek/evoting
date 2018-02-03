using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Common
{
    public class Client : IClient
    {
        private const int MAX_MSG_LEN = 4096; 
        private TcpClient client = new TcpClient();
        private NetworkStream stream;
        private Thread clientThread;
        public event NewMessageHandler OnNewMessageReceived;

        public bool Connected
        {
            get { return client.Connected; }
        }
        
        public Client() {}

        public bool connect(string ip, string port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            int dstPort = Convert.ToInt32(port);

            try
            {
                client.Connect(new IPEndPoint(ipAddress, dstPort));
            }
            catch { }
            
            if (client.Connected)
            {
                stream = client.GetStream();
                clientThread = new Thread(new ThreadStart(displayMessageReceived));
                clientThread.Start();
                return true;
            }
            else
            {
                // client = null;
                return false;
            }
        }

        private void displayMessageReceived()
        {
            byte[] message = new byte[MAX_MSG_LEN];
            int bytesRead;

            while (stream.CanRead)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(message, 0, MAX_MSG_LEN);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string signal = new ASCIIEncoding().GetString(message, 0, bytesRead);
                Console.WriteLine(message);
                MessageArgs myArgs = new MessageArgs(signal);
                OnNewMessageReceived(myArgs);
            }
            disconnect();
        }

        public void disconnect()
        {
            try
            {
                client.GetStream().Close();
                client.Close();
                // clientThread.Join();
            }
            catch
            {
                Console.WriteLine("Cannot disconnect from server");
            }
        }

        public void sendMessage(string msg)
        {
            if (client != null && client.Connected && msg != "")
            {
                byte[] buffer = new ASCIIEncoding().GetBytes(msg);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }
        public void subscribe(NewMessageHandler handler)
        {
            OnNewMessageReceived+=handler;
        }
    }
}