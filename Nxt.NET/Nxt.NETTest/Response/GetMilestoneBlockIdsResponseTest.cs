using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Response;

namespace Nxt.NETTest.Response
{
    [TestClass]
    public class GetMilestoneBlockIdsResponseTest
    {
        private const string ZeroMilestoneJson = "{\"milestoneBlockIds\":[]}";
        private const string OneMilestoneJson = "{\"milestoneBlockIds\":[\"2680262203532249785\"]}";
        private const string LastMilestoneJson = "{\"milestoneBlockIds\":[\"2680262203532249785\"],\"last\":\"true\"}";

        [TestMethod]
        public void ShouldReturnZeroBlockIds()
        {
            var response = new GetMilestoneBlockIdsResponse(ZeroMilestoneJson);

            Assert.AreEqual(0, response.MilestonBlockIds.Count);
            Assert.IsFalse(response.IsLast);
        }

        [TestMethod]
        public void ShouldReturnOneBlockId()
        {
            var response = new GetMilestoneBlockIdsResponse(OneMilestoneJson);

            var id = response.MilestonBlockIds.Single();
            Assert.AreEqual(2680262203532249785L, id);
            Assert.IsFalse(response.IsLast);
        }

        [TestMethod]
        public void ShouldReturnOneBlockIdAndSetIsLast()
        {
            var response = new GetMilestoneBlockIdsResponse(LastMilestoneJson);

            var id = response.MilestonBlockIds.Single();
            Assert.AreEqual(2680262203532249785L, id);
            Assert.IsTrue(response.IsLast);
        }
    }
}
