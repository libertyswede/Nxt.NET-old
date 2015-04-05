using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;

namespace Nxt.NETTest
{
    [TestClass]
    public class AliasTest
    {
        [TestMethod]
        public void TestProperties()
        {
            var account = new Mock<IAccount>().Object;
            const int id = 1234;
            const string name = "Da Name";
            const string uri = "some uri";
            const int timestamp = 7890;

            var alias = new Alias(account, id, name, uri, timestamp);

            Assert.AreSame(account, alias.Account);
            Assert.AreEqual(id, alias.Id);
            Assert.AreEqual(name, alias.Name);
            Assert.AreEqual(uri, alias.Uri);
            Assert.AreEqual(timestamp, alias.Timestamp);
        }
    }
}
