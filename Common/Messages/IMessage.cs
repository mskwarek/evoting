namespace Common.Messages
{
    public interface IMessage
    {
         string getHeader();
         string getPayload();
    }
}