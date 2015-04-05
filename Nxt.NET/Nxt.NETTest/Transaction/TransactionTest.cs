using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionTest
    {
        private Mock<ITransactionByteSerializer> _transactionByteSerializerMock;
        private Mock<IAccountContainer> _accountContainerMock;
        private NET.Transaction.Transaction _transaction;
        private Mock<ITransactionType> _transactionTypeMock;

        [TestInitialize]
        public void TestInit()
        {
            _transactionByteSerializerMock = new Mock<ITransactionByteSerializer>();
            _accountContainerMock = new Mock<IAccountContainer>();
            _transactionTypeMock = new Mock<ITransactionType>();

            _transaction = new NET.Transaction.Transaction(_transactionByteSerializerMock.Object, _accountContainerMock.Object)
            {
                TransactionType = _transactionTypeMock.Object
            };
        }

        [TestMethod]
        public void GetExpirationTest()
        {
            _transaction.Timestamp = 100;
            _transaction.Deadline = 60;

            var actual = _transaction.GetExpiration();

            Assert.AreEqual(3700, actual);
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldReturnFalseWhenSenderAccountDoesNotExist()
        {
            _transaction.SenderId = 42;
            _accountContainerMock.Setup(ac => ac.GetAccount(42)).Returns((IAccount)null);
            
            var actual = _transaction.ApplyUnconfirmed();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldReturnFalseWhenNotApplied()
        {
            var account = new Mock<IAccount>().Object;
            _transaction.SenderId = 42;
            _accountContainerMock.Setup(ac => ac.GetAccount(42)).Returns(account);
            _transactionTypeMock.Setup(tt => tt.ApplyUnconfirmed(It.IsAny<ITransaction>(), It.IsAny<IAccount>())).Returns(false);

            var actual = _transaction.ApplyUnconfirmed();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ApplyUnconfirmedShouldReturnTrueWhenApplied()
        {
            var account = new Mock<IAccount>().Object;
            var transactionTypeMock = new Mock<ITransactionType>();
            _transaction.SenderId = 42;
            _accountContainerMock.Setup(ac => ac.GetAccount(42)).Returns(account);
            transactionTypeMock.Setup(tt => tt.ApplyUnconfirmed(It.IsAny<ITransaction>(), It.IsAny<IAccount>())).Returns(true);
            _transaction.TransactionType = transactionTypeMock.Object;

            var actual = _transaction.ApplyUnconfirmed();

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ApplyShouldCallApplyPublicKey()
        {
            var senderAccountMock = new Mock<IAccount>();
            _transaction.Height = 42;
            _transaction.SenderId = 43;
            _transaction.SenderPublicKey = new byte[] {1, 2, 3};
            _accountContainerMock.Setup(ac => ac.GetAccount(It.Is<long>(id => id == 43))).Returns(senderAccountMock.Object);

            _transaction.Apply();

            senderAccountMock.Verify(a => a.ApplyPublicKey(It.IsAny<byte[]>(), It.IsAny<int>()));
            _transactionTypeMock.Verify(tt => tt.Apply(It.IsAny<ITransaction>(), It.IsAny<IAccount>(), It.IsAny<IAccount>()));
        }

        [TestMethod]
        public void PresentationIdShouldBePositive()
        {
            _transaction.Id = -2;

            Assert.AreEqual(UInt64.MaxValue - 1, _transaction.PresentationId);
        }

        [TestMethod]
        public void UseNQTShouldUseByteSerializer()
        {
            _transactionByteSerializerMock.Setup(s => s.UseNQT(It.IsAny<ITransaction>())).Returns(true);

            var actual = _transaction.UseNQT();

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateAttachmentShouldUseTransactionTYpe()
        {
            _transaction.ValidateAttachment();

            _transactionTypeMock.Verify(tt => tt.ValidateAttachment(It.IsAny<ITransaction>()));
        }

        [TestMethod]
        public void IsDuplicateShouldBeFalse()
        {
            _transaction.IsDuplicate(new Dictionary<ITransactionType, HashSet<string>>());

            _transactionTypeMock.Verify(tt => tt.IsDuplicate(
                It.IsAny<ITransaction>(), 
                It.IsAny<IDictionary<ITransactionType, HashSet<string>>>()));
        }

        [TestMethod]
        public void UndoUnconfirmed()
        {
            _transaction.UndoUnconfirmed();

            _transactionTypeMock.Verify(tt => tt.UndoUnconfirmed(It.IsAny<ITransaction>(), It.IsAny<IAccount>()));
        }
    }
}
