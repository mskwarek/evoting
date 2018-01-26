using Common.Messages;

namespace voter
{
    public class StateIdle : Common.State
    {
        override public void getNextState()
        {
            
        }

        override public void handleMessage(IMessage message)
        {
            Common.Logger.log(message.getPayload());
        }
    }
}