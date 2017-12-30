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
        Voter cut;
        public VoterUnitTest()
        {
            fileHelper.Setup(x=>x.DirectoryExists(It.IsAny<string>())).Returns(true);
            
            cut = new Voter(fileHelper.Object);
        }

        [Fact]
        public void ExceptionIsRaisedWhenPathPassedToReadConfigIsNotValid()
        {
            string wrongPath = "blahblahblah";
            fileHelper.Setup(x=>x.ReadAllText(It.Is<string>(s => s.Equals(wrongPath)))).Throws(new FileNotFoundException());

            Assert.Throws<FileNotFoundException>(() => cut.readConfiguration(wrongPath));
        }

        [Fact]
        public void FileIsLoadedWhenRightPathIsPassed()
        {
            string rightPath = "rightPath";
            fileHelper.Setup(x=>x.ReadAllText(It.Is<string>(s => s.Equals(rightPath)))).Returns("{}");

            cut.readConfiguration(rightPath); 
        }

        [Fact]
        public void fileIsRightParsedFromProperJson()
        {
            string jsonConfigPat = "json";
            string properJson = "{ \"Id\" : 1, \"eaIp\" : \"127.0.0.1\", \"eaPort\" : \"5001\", \"proxyIp\" : \"127.0.0.1\", \"proxyPort\" : \"5002\", \"name\" : \"Voter1\"}"
            fileHelper.Setup(x => x.ReadAllText(It.Is<string>(s => s.Equals(jsonConfigPath)))).Returns(properJson);
        
            cut.readConfiguration(jsonConfigPath);
            
        }
    }
}
