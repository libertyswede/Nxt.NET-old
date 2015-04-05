using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;

namespace Nxt.NETTest.Transaction.Types
{
    [TestClass]
    public class ArbitraryMessageTest
    {
        private ArbitraryMessage _arbitraryMessage;
        private Mock<ITransaction> _transaction;

        [TestInitialize]
        public void TestInit()
        {
            _transaction = new Mock<ITransaction>();

            _arbitraryMessage = new ArbitraryMessage(new Mock<IConfiguration>().Object);
        }

        [TestMethod]
        public void GetSubtypeByte()
        {
            Assert.AreEqual(0, _arbitraryMessage.GetSubtypeByte());
        }

        [TestMethod]
        public void LoadAttachment()
        {
            _transaction.SetupProperty(t => t.Attachment);
            var attachmentData = new byte[]
            {
                5, 0, 0, 0, // length
                1, 2, 3, 4, 5 // message
            };

            _arbitraryMessage.LoadAttachment(_transaction.Object, attachmentData);
            var attachment = (IArbitraryMessageAttachment) _transaction.Object.Attachment;

            CollectionAssert.AreEqual(new byte[]{1, 2, 3, 4, 5}, attachment.MessageBytes);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void LoadAttachmentShouldFailWhenMessageLengthIsTooLarge()
        {
            var attachmentData = new byte[] { 0, 5, 0, 0 }; // 5 * 255 

            _arbitraryMessage.LoadAttachment(_transaction.Object, attachmentData);
        }

        [TestMethod]
        public void LoadAttachmentJTokenShouldSetCorrectMessage()
        {
            _transaction.SetupProperty(t => t.Attachment);
            var expected = new byte[]
            {
                181, 87, 132, 12, 116, 201, 217, 7, 116, 106, 130, 38, 254, 222, 130, 94, 145, 251, 152, 108, 242, 254, 56,
                132, 182, 235, 98, 134, 123, 213, 134, 51
            };
            const string hexString = "b557840c74c9d907746a8226fede825e91fb986cf2fe3884b6eb62867bd58633";
            var jtoken = JObject.Parse("{\"message\":\"" + hexString + "\"}");
            
            _arbitraryMessage.LoadAttachment(_transaction.Object, jtoken);
            var attachment = (IArbitraryMessageAttachment) _transaction.Object.Attachment;

            CollectionAssert.AreEqual(expected, attachment.MessageBytes);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenMessageIsTooLong()
        {
            var attachmentMock = new Mock<IArbitraryMessageAttachment>();
            attachmentMock.SetupGet(a => a.MessageBytes).Returns(new byte[Constants.MaxArbitraryMessageLength + 1]);
            _transaction.SetupGet(t => t.Attachment).Returns(attachmentMock.Object);

            _arbitraryMessage.ValidateAttachment(_transaction.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAmountIsNotZero()
        {
            _transaction.SetupGet(t => t.AmountNQT).Returns(1);

            _arbitraryMessage.ValidateAttachment(_transaction.Object);
        }
    }
}
