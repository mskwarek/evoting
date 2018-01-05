using Xunit;
using voter;
using Moq;
using Common;
using System;

namespace voterUnitTest
{
    public class VoterToElectionAuthMessagesUnitTest : VoterMessagesUnitTest
    {
        
        [Fact]
        public void requestForCandidateListIsProperlySent()
        {
            var expectedMessagePattern = String.Format("{{\"HEADER\":\"{0}\",\"senderName\":\"{1}\",\"slValue\":\"{2}\"}}",
                                    Common.Messages.Headers.CANDIDATE_LIST_HEADER_REQ, exampleName,
                                    @"\d+");
            readVoterConfig();
            voter.requestForCandidateListFromElectionAuth();

            electionAuthorityTransportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => System.Text.RegularExpressions.Regex.IsMatch(s, expectedMessagePattern))), 
                Times.Once());
        }
    }
}