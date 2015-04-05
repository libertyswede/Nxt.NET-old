using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Response;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetNextBlocksResponseTest
    {
        private const string MinimumJson = "{nextBlocks:[]}";

        private const string ThreeBlockJson =  
            "{nextBlocks:[" +
                "{\"test\": \"test\"}," + 
                "{\"test\": \"test\"}," + 
                "{\"test\": \"test\"}," +
            "]}";

        [TestMethod]
        public void GetNextBlocksResponseShouldReturnThreeBlocks()
        {
            var blockMock = new Mock<IBlock>();
            var blockParserMock = new Mock<IBlockParser>();
            blockParserMock.Setup(bp => bp.ParseBlock(It.IsAny<JToken>()))
                .Returns(blockMock.Object);

            var response = new GetNextBlocksResponse(blockParserMock.Object, ThreeBlockJson);

            blockParserMock.Verify(bp => bp.ParseBlock(It.IsAny<JToken>()), Times.Exactly(3));
            Assert.AreEqual(3, response.Blocks.Count);
            response.Blocks.ToList().ForEach(b => Assert.AreEqual(blockMock.Object, b));
        }

        [TestMethod]
        public void GetNextBlocksResponseShouldReturnZeroBlocks()
        {
            var blockParserMock = new Mock<IBlockParser>();

            var response = new GetNextBlocksResponse(blockParserMock.Object, MinimumJson);

            blockParserMock.Verify(bp => bp.ParseBlock(It.IsAny<JToken>()), Times.Never);
            Assert.AreEqual(0, response.Blocks.Count);
        }
    }
}
