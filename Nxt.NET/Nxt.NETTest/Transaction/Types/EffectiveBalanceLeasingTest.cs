using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;

namespace Nxt.NETTest.Transaction.Types
{
    [TestClass]
    public class EffectiveBalanceLeasingTest
    {
        private EffectiveBalanceLeasing _effectiveBalanceLeasing;
        private Mock<ITransaction> _transaction;
        private Mock<IAccountContainer> _accountContainer;

        [TestInitialize]
        public void TestInit()
        {
            _transaction = new Mock<ITransaction>();
            _accountContainer = new Mock<IAccountContainer>();

            _effectiveBalanceLeasing = new EffectiveBalanceLeasing(new Mock<IConfiguration>().Object,
                _accountContainer.Object);
        }

        [TestMethod]
        public void GetSubtypeByte()
        {
            var actual = _effectiveBalanceLeasing.GetSubtypeByte();

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void LoadAttachmentBytes()
        {
            var attachmentBytes = new byte[] {1, 10};
            const short expected = (short) 1 + (256*10);
            _transaction.SetupProperty(t => t.Attachment);

            _effectiveBalanceLeasing.LoadAttachment(_transaction.Object, attachmentBytes);

            var attachment = (IEffectiveBalanceLeasingAttachment) _transaction.Object.Attachment;
            Assert.AreEqual(expected, attachment.Period);
        }

        [TestMethod]
        public void LoadAttachmentJToken()
        {
            var attachmentData = JObject.Parse("{\"period\":\"2561\"}");
            _transaction.SetupProperty(t => t.Attachment);

            _effectiveBalanceLeasing.LoadAttachment(_transaction.Object, attachmentData);

            var attachment = (IEffectiveBalanceLeasingAttachment)_transaction.Object.Attachment;
            Assert.AreEqual(2561, attachment.Period);
        }

        [TestMethod]
        public void ValidateAttachment()
        {
            SetupValidAttachmentForValidation();

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }

        // Dunno what's so special with this TX id, but it's in the NRS client
        [TestMethod]
        public void ValidateAttachmentForSpecialTransaction()
        {
            SetupValidAttachmentForValidation();
            _transaction.SetupGet(t => t.PresentationId).Returns(5081403377391821646);
            var account = new Mock<IAccount>();
            account.SetupGet(a => a.PublicKey).Returns((byte[]) null);
            _accountContainer.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 1))).Returns(account.Object);

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }

        private void SetupValidAttachmentForValidation()
        {
            var attachment = new Mock<IEffectiveBalanceLeasingAttachment>();
            var account = new Mock<IAccount>();
            attachment.SetupGet(a => a.Period).Returns(1441);
            _transaction.SetupGet(t => t.RecipientId).Returns(1);
            _transaction.SetupGet(t => t.SenderId).Returns(2);
            _transaction.SetupGet(t => t.AmountNQT).Returns(0);
            _transaction.SetupGet(t => t.Attachment).Returns(attachment.Object);
            _accountContainer.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 1))).Returns(account.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldRefuseWhenLeasingToMyself()
        {
            SetupValidAttachmentForValidation();
            _transaction.SetupGet(t => t.RecipientId).Returns(1);
            _transaction.SetupGet(t => t.SenderId).Returns(1);

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldRefuseAmountIsNonZero()
        {
            SetupValidAttachmentForValidation();
            _transaction.SetupGet(t => t.AmountNQT).Returns(42);

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldRefuseTooLowLeasingPeriod()
        {
            SetupValidAttachmentForValidation();
            var attachment = new Mock<IEffectiveBalanceLeasingAttachment>();
            attachment.SetupGet(a => a.Period).Returns(1);
            _transaction.SetupGet(t => t.Attachment).Returns(attachment.Object);

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldRefuseUnknownRecipientAccount()
        {
            SetupValidAttachmentForValidation();
            _accountContainer.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 1))).Returns((IAccount) null);

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }

        // Dunno what's so special with this TX id, but it's in the NRS client
        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldRefuseUnknownPublicKey()
        {
            SetupValidAttachmentForValidation();
            var account = new Mock<IAccount>();
            account.SetupGet(a => a.PublicKey).Returns((byte[])null);
            _accountContainer.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 1))).Returns(account.Object);

            _effectiveBalanceLeasing.ValidateAttachment(_transaction.Object);
        }
    }
}
