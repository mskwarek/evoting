using System;
using System.Collections.Generic;

namespace Common.Messages
{
    public class CandidateListResp
    {
        string HEADER = Headers.CANDIDATE_LIST_HEADER_RESP;
        string senderName;
        List<string> candidateList;
    }
}