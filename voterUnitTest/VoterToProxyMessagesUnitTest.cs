using Xunit;
using voter;
using Moq;
using Common;
using Newtonsoft.Json;
using System;

namespace voterUnitTest
{
    public class VoterToProxyMessagesUnitTest : VoterMessagesUnitTest
    {
        [Fact]
        public void requestForSrAndSlIsProperlySentWithoutNameCauseOfNoConfiguration()
        {    
            voter.requestForSrAndSlFromProxy();
            proxyTransportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => s.Contains(Common.Messages.Headers.SR_SL_HEADER_REQ))), 
                Times.Once());
        }

        [Fact]
        public void requestForSrAndSlIsProperlySentWithName()
        {
            var expectedJson = String.Format("{{\"HEADER\":\"{0}\",\"senderName\":\"{1}\"}}", 
                            Common.Messages.Headers.SR_SL_HEADER_REQ, exampleName);

            readVoterConfig();
            voter.requestForSrAndSlFromProxy();

            proxyTransportLayerMock.Verify(mock => mock.sendMessage(
                    It.Is<string>(s => s.Equals(expectedJson))), 
                Times.Once());
        }
    }
}