using System.IO;
using Xunit;
using voter;
using Moq;
using Common;

namespace voterUnitTest
{
    public class VoterUnitTest
    {
        Mock<IFileHelper> fileHelper = new Mock<IFileHelper>();
        Mock<IClient> proxyTransportLayerMock = new Mock<IClient>();
        Voter cut;
        public VoterUnitTest()
        {
            cut = new Voter(proxyTransportLayerMock.Object);
            fileHelper.Setup(x=>x.DirectoryExists(It.IsAny<string>())).Returns(true);
        }

        [Fact]
        public void ExceptionIsRaisedWhenPathPassedToReadConfigIsNotValid()
        {
            string wrongPath = "blahblahblah";
            fileHelper.Setup(x=>x.ReadAllText(It.Is<string>(s => s.Equals(wrongPath)))).Throws(new FileNotFoundException());

            Assert.Throws<FileNotFoundException>(() => cut.readConfiguration(fileHelper.Object, wrongPath));
        }

        [Fact]
        public void FileIsLoadedWhenRightPathIsPassed()
        {
            string rightPath = "rightPath";
            fileHelper.Setup(x=>x.ReadAllText(It.Is<string>(s => s.Equals(rightPath)))).Returns("{}");

            cut.readConfiguration(fileHelper.Object, rightPath); 
        }

        [Fact]
        public void fileIsRightParsedFromProperJson()
        {
            string jsonConfigPath = "json";
            string properJson = "{ \"Id\" : 1, \"eaIp\" : \"127.0.0.1\", \"eaPort\" : \"5001\", \"proxyIp\" : \"127.0.0.1\", \"proxyPort\" : \"5002\", \"name\" : \"Voter1\"}";
            fileHelper.Setup(x => x.ReadAllText(It.Is<string>(s => s.Equals(jsonConfigPath)))).Returns(properJson);
        
            cut.readConfiguration(fileHelper.Object, jsonConfigPath);
            Assert.Equal("127.0.0.1", cut.voterConfig.eaIp);
            Assert.Equal("Voter1", cut.voterConfig.name);
            Assert.Equal("5001", cut.voterConfig.eaPort);
            Assert.Equal("127.0.0.1", cut.voterConfig.proxyIp);
            Assert.Equal("5002", cut.voterConfig.proxyPort);
        }
    }
}
