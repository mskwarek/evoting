namespace Common
{
    public interface IClient
    {
        void sendMessage(string message);   
        void disconnect();
        bool connect(string ip, string port);
    }
}