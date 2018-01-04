using Xunit;
using voter;
using Moq;
using Common;

namespace voterUnitTest
{
    public class VoterToProxyMessagesUnitTest
    {
        [Fact]
        public void requestForSrAndSlIsProperlySent()
        {
            var transportLayerMock = new Mock<IClient>();
            var voter = new Voter(transportLayerMock.Object);
            
            voter.requestForSrAndSl();
            transportLayerMock.Verify(mock => mock.sendMessage(
                It.Is<string>(s => s.Contains(Common.Messages.Headers.SR_SL_HEADER_REQ))), 
                Times.Once());
        }
    }
}