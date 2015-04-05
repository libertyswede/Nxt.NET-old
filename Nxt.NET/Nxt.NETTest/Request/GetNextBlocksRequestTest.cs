using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Request;

namespace Nxt.NETTest.Request
{
    [TestClass]
    public class GetNextBlocksRequestTest : AddQuoteConverterTest
    {
        [TestMethod]
        public void SerializingToJsonShouldQuoteLongValues()
        {
            SetupTest();

            var request = new GetNextBlocksRequest(Id);

            AssertTest("blockId", request);
        }
    }
}
