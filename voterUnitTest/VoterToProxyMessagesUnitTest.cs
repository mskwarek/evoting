using Xunit;
using voter;
using Moq;
using Common;
using Newtonsoft.Json;

namespace voterUnitTest
{
    public class VoterToProxyMessagesUnitTest
    {

        Mock<IClient> transportLayerMock = new Mock<IClient>();
        Voter voter;
        
        public VoterToProxyMessagesUnitTest()
        {
            voter = new Voter(transportLayerMock.Object);
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
            var exampleName = "example";
            var configJson = "{ \"name\" : \""+exampleName+"\" }";
            var expectedJson = "{\"HEADER\":\""+Common.Messages.Headers.SR_SL_HEADER_REQ+"\",\"senderName\":\""+exampleName+"\"}";
            Mock<IFileHelper> fileHelper = new Mock<IFileHelper>();
            fileHelper.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(configJson);

            voter.readConfiguration(fileHelper.Object, "someString");
            voter.requestForSrAndSl();

            transportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => s.Equals(expectedJson))), 
                Times.Once());
        }
    }
}