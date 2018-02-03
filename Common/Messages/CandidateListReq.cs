namespace Common.Messages
{
    public class CandidateListReq : public IMessage
    {
        public string HEADER = Headers.CANDIDATE_LIST_HEADER_REQ;
        public string senderName = "";
        public string slValue = "";
    }
}