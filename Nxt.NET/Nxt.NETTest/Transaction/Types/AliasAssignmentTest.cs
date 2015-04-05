using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;

namespace Nxt.NETTest.Transaction.Types
{
    [TestClass]
    public class AliasAssignmentTest
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<IAliasContainer> _aliasContainerMock;
        private AliasAssignment _aliasAssignment;
        private Mock<ITransaction> _transactionMock;
        private Mock<IAccount> _senderAccountMock;
        private Mock<IAccount> _recipientAccountMock;
        private static Mock<IAliasAssignmentAttachment> _attachmentMock;
        private byte[] _attachmentData;

        [TestInitialize]
        public void TestInit()
        {
            _configurationMock = new Mock<IConfiguration>();
            _aliasContainerMock = new Mock<IAliasContainer>();
            _senderAccountMock = new Mock<IAccount>();
            _recipientAccountMock = new Mock<IAccount>();
            _transactionMock = new Mock<ITransaction>();
            _attachmentMock = new Mock<IAliasAssignmentAttachment>();

            _senderAccountMock.SetupGet(a => a.Balance).Returns(new Mock<IAccountBalance>().Object);
            _attachmentMock.SetupGet(a => a.AliasName).Returns("testalias");
            _attachmentMock.SetupGet(a => a.AliasUri).Returns("testuri");
            _transactionMock.SetupProperty(t => t.Attachment);
            _transactionMock.SetupGet(t => t.RecipientId).Returns(Genesis.CreatorId);
            _attachmentData = new byte[] 
            {
                9, // aliasLength
                116, 101, 115, 116, 97, 108, 105, 97, 115, // string "testalias" as UTF8
                10, 0, // uriLength
                103, 111, 111, 103, 108, 101, 46, 99, 111, 109 // string "google.com" as UTF8
            };

            _aliasAssignment = new AliasAssignment(_configurationMock.Object, _aliasContainerMock.Object);
        }

        [TestMethod]
        public void GetSubtypeByteShouldBeCorrect()
        {
            var actual = _aliasAssignment.GetSubtypeByte();

            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public void LoadAttachmentShouldCreateAliasAssignmentAttachment()
        {
            var attachmentData = JObject.Parse("{\"alias\":\"testalias\",\"uri\":\"google.com\"}");

            _aliasAssignment.LoadAttachment(_transactionMock.Object, attachmentData);

            VerifyAliasAssignmentAttachment();
        }

        [TestMethod]
        public void LoadAttachmentBytesShouldCreateAliasAssignmentAttachment()
        {
            _aliasAssignment.LoadAttachment(_transactionMock.Object, _attachmentData);

            VerifyAliasAssignmentAttachment();
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void LoadAttachmentBytesShouldFailWhenAliasIsTooLong()
        {
            _attachmentData[0] = Constants.MaxAliasLength + 1;

            _aliasAssignment.LoadAttachment(_transactionMock.Object, _attachmentData);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void LoadAttachmentBytesShouldFailWhenAliasUriIsTooLong()
        {
            _attachmentData[10] = 0;
            _attachmentData[11] = 5;

            _aliasAssignment.LoadAttachment(_transactionMock.Object, _attachmentData);
        }

        [TestMethod]
        public void ApplyAttachmentShouldCallAddOrUpdateOnAliasContainer()
        {
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.Apply(_transactionMock.Object, _senderAccountMock.Object, _recipientAccountMock.Object);

            _aliasContainerMock.Verify(ac => ac.AddOrUpdate(It.Is<IAccount>(a => a == _senderAccountMock.Object),
                It.Is<ITransaction>(t => t == _transactionMock.Object),
                It.Is<IAliasAssignmentAttachment>(a => a == _attachmentMock.Object)));
        }

        [TestMethod]
        public void UndoAttachmentShouldRemoveAliasFromContainer()
        {
            var aliasMock = new Mock<IAlias>();
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);
            _aliasContainerMock.Setup(ac => ac.GetAlias(It.Is<string>(name => name == _attachmentMock.Object.AliasName)))
                .Returns(aliasMock.Object);

            _aliasAssignment.Undo(_transactionMock.Object, _senderAccountMock.Object, _recipientAccountMock.Object);
            
            _aliasContainerMock.Verify(ac => ac.Remove(It.Is<IAlias>(a => a == aliasMock.Object)));
        }

        [TestMethod]
        [ExpectedException(typeof(UndoNotSupportedException))]
        public void UndoAttachmentShouldShouldNotAcceptUpdatedAlias()
        {
            var aliasMock = new Mock<IAlias>();
            aliasMock.SetupGet(a => a.Id).Returns(42);
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);
            _aliasContainerMock.Setup(ac => ac.GetAlias(It.Is<string>(name => name == _attachmentMock.Object.AliasName)))
                .Returns(aliasMock.Object);

            _aliasAssignment.Undo(_transactionMock.Object, _senderAccountMock.Object, _recipientAccountMock.Object);

            _aliasContainerMock.Verify(ac => ac.Remove(It.Is<IAlias>(a => a == aliasMock.Object)));
        }

        [TestMethod]
        public void IsDuplicateShouldReturnFalse()
        {
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);
            var duplicates = new Dictionary<ITransactionType, HashSet<string>>();

            var actual = _aliasAssignment.IsDuplicate(_transactionMock.Object, duplicates);

            Assert.IsFalse(actual);
            var hashSet = duplicates[_aliasAssignment];
            Assert.IsTrue(hashSet.Contains(_attachmentMock.Object.AliasName.ToLower()));
        }

        [TestMethod]
        public void IsDuplicateShouldReturnTrue()
        {
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);
            var duplicates = new Dictionary<ITransactionType, HashSet<string>>();

            _aliasAssignment.IsDuplicate(_transactionMock.Object, duplicates);
            var actual = _aliasAssignment.IsDuplicate(_transactionMock.Object, duplicates);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateAttachmentShouldSucceed()
        {
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenRecipientIsNotGenesisCreator()
        {
            _transactionMock.SetupGet(t => t.RecipientId).Returns(42);
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAmountIsNotZero()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(1);
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAliasNameLengthIsZero()
        {
            _attachmentMock.SetupGet(a => a.AliasName).Returns(string.Empty);
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAliasNameLengthIsTooLong()
        {
            _attachmentMock.SetupGet(a => a.AliasName).Returns(new string('1', Constants.MaxAliasLength + 1));
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAliasUriLengthIsTooLong()
        {
            _attachmentMock.SetupGet(a => a.AliasUri).Returns(new string('1', Constants.MaxAliasUriLength + 1));
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAliasNameContainIllegalCharacter()
        {
            _attachmentMock.SetupGet(a => a.AliasName).Returns("å");
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtValidationException))]
        public void ValidateAttachmentShouldFailWhenAliasAlreadyOwnedByAnotherAccount()
        {
            var aliasMock = new Mock<IAlias>();
            var accountMock = new Mock<IAccount>();
            _aliasContainerMock.Setup(ac => ac.GetAlias(It.IsAny<string>())).Returns(aliasMock.Object);
            aliasMock.SetupGet(a => a.Account).Returns(accountMock.Object);
            accountMock.SetupGet(a => a.PublicKey).Returns(new byte[] {1, 2, 3});
            _transactionMock.SetupGet(t => t.SenderPublicKey).Returns(new byte[] {2, 3, 4});
            _transactionMock.SetupGet(t => t.Attachment).Returns(_attachmentMock.Object);

            _aliasAssignment.ValidateAttachment(_transactionMock.Object);
        }

        private void VerifyAliasAssignmentAttachment()
        {
            var attachment = (AliasAssignmentAttachment)_transactionMock.Object.Attachment;
            Assert.AreEqual("testalias", attachment.AliasName);
            Assert.AreEqual("google.com", attachment.AliasUri);
        }
    }
}
