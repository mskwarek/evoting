namespace Common.Messages
{
    public static class MessageBuilder
    {
        public static IMessage buildMessage(string rawMessage)
        {
            return new CandidateListReq();
        }
    }
}