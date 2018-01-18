namespace Common
{
    public delegate void NewMessageHandler(Common.Messages.IMessage message);

    public interface IClient
    {    
        void sendMessage(string message);   
        void disconnect();
        bool connect(string ip, string port);
        void subscribe(NewMessageHandler handler);
    }
}