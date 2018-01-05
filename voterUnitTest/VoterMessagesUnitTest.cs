using Xunit;
using voter;
using Moq;
using Common;
using System;

namespace voterUnitTest
{
    public class VoterMessagesUnitTest
    {
        
        protected Mock<IClient> proxyTransportLayerMock = new Mock<IClient>();
        protected Mock<IClient> electionAuthorityTransportLayerMock = new Mock<IClient>();
        protected Voter voter;
        protected string exampleName = "example";

        public VoterMessagesUnitTest()
        {
            voter = new Voter(proxyTransportLayerMock.Object, 
                        electionAuthorityTransportLayerMock.Object);
        }

        protected void readVoterConfig()
        {           
            readConfigForSpecificVoter(voter);
        }

        protected void readConfigForSpecificVoter(Voter voter)
        {
            var configJson = String.Format("{{ \"name\" : \"{0}\" }}", exampleName);
            Mock<IFileHelper> fileHelper = new Mock<IFileHelper>();

            fileHelper.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(configJson);
            voter.readConfiguration(fileHelper.Object, "someString");
        }
    }
}