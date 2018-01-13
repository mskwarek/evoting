using Common.Messages;
using System;

namespace Common
{
    public abstract class State
    {
        virtual public void handleMessage(IMessage message)
        {
            Common.Logger.log(String.Format("Unknown message received in this state {0}", message.getHeader()));
        }
        public abstract void getNextState();
    }
}