using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class AliasAssignmentAttachmentTest
    {
        private AliasAssignmentAttachment _aliasAssignmentAttachment;
        private const string AliasName = "testalias";
        private const string AliasUri = "google.com";
        

        [TestInitialize]
        public void TestInit()
        {
            _aliasAssignmentAttachment = new AliasAssignmentAttachment(AliasName, AliasUri);
        }

        [TestMethod]
        public void LengthShouldBeCorrect()
        {
            Assert.AreEqual(22, _aliasAssignmentAttachment.Length);
        }

        [TestMethod]
        public void GetBytesShouldReturnCorrectArray()
        {
            byte[] expectedBytes =
            {
                9, // aliasLength
                116, 101, 115, 116, 97, 108, 105, 97, 115, // string "testalias" as UTF8
                10, 0, // uriLength
                103, 111, 111, 103, 108, 101, 46, 99, 111, 109 // string "google.com" as UTF8
            };

            CollectionAssert.AreEqual(expectedBytes, _aliasAssignmentAttachment.GetBytes());
        }
    }
}
