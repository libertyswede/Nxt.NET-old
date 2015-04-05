using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class ArbitraryMessageAttachmentTest
    {
        private ArbitraryMessageAttachment _arbitraryMessageAttachment;
        private readonly byte[] _messageBytes = {1, 2, 3};

        [TestInitialize]
        public void TestInit()
        {
            _arbitraryMessageAttachment = new ArbitraryMessageAttachment(_messageBytes);
        }

        [TestMethod]
        public void LengthShouldBeCorrect()
        {
            Assert.AreEqual(7, _arbitraryMessageAttachment.Length);
        }

        [TestMethod]
        public void GetBytesShouldReturnExpectedByteArray()
        {
            var expected = new byte[] {3, 0, 0, 0, 1, 2, 3};

            var actual = _arbitraryMessageAttachment.GetBytes();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MEssageShouldReturnExpectedByteArray()
        {
            CollectionAssert.AreEqual(_messageBytes, _arbitraryMessageAttachment.MessageBytes);
        }
    }
}
