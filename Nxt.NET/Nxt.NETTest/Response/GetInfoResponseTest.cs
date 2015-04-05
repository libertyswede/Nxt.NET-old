using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Response;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetInfoResponseTest
    {
        private const string ValidInfoResponse =
            "{" +
                "\"shareAddress\":true," +
                "\"platform\":\"1J6R8X4mcU\"," +
                "\"application\":\"NRS\"," +
                "\"announcedAddress\":\"151.236.29.228\"," +
                "\"version\":\"1.1.3\"" +
            "}";

        [TestMethod]
        public void ShouldSetCorrectValues()
        {
            var response = new GetInfoResponse(ValidInfoResponse);

            Assert.AreEqual(true, response.ShareAddress);
            Assert.AreEqual("1J6R8X4mcU", response.Platform);
            Assert.AreEqual("NRS", response.Application);
            Assert.AreEqual("151.236.29.228", response.AnnouncedAddress);
            Assert.AreEqual("1.1.3", response.Version);
            Assert.IsNull(response.Hallmark);
        }
    }
}
