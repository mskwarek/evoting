using Xunit;
using voter;
using Moq;
using Common;
using Newtonsoft.Json;
using System;

namespace voterUnitTest
{
    public class VoterToProxyMessagesUnitTest
    {

        Mock<IClient> transportLayerMock = new Mock<IClient>();
        Voter voter;
        string exampleName = "example";

        public VoterToProxyMessagesUnitTest()
        {
            voter = new Voter(transportLayerMock.Object);
        }

        private void readVoterConfig()
        {           
            var configJson = String.Format("{{ \"name\" : \"{0}\" }}", exampleName);
            Mock<IFileHelper> fileHelper = new Mock<IFileHelper>();

            fileHelper.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(configJson);
            voter.readConfiguration(fileHelper.Object, "someString");
        }

        [Fact]
        public void requestForSrAndSlIsProperlySentWithoutNameCauseOfNoConfiguration()
        {    
            voter.requestForSrAndSl();
            transportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => s.Contains(Common.Messages.Headers.SR_SL_HEADER_REQ))), 
                Times.Once());
        }

        [Fact]
        public void requestForSrAndSlIsProperlySentWithName()
        {
            var expectedJson = String.Format("{{\"HEADER\":\"{0}\",\"senderName\":\"{1}\"}}", 
                            Common.Messages.Headers.SR_SL_HEADER_REQ, exampleName);

            readVoterConfig();
            voter.requestForSrAndSl();

            transportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => s.Equals(expectedJson))), 
                Times.Once());
        }

        [Fact]
        public void requestForCandidateListIsProperlySent()
        {
            var expectedMessage = String.Format("{{\"HEADER\":\"{0}\",\"senderName\":\"{1}\",\"slValue\":\"{2}\"}}",
                                    Common.Messages.Headers.CANDIDATE_LIST_HEADER_REQ, exampleName,
                                    It.Is<string>(s => s.Equals("sadsa")));
            // voter.requestForCandidateList();

            transportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => s.Equals(expectedMessage))), 
                Times.Once());
        }
    }
}