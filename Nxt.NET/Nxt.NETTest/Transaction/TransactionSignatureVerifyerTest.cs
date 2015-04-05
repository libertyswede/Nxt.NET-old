using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionSignatureVerifyerTest
    {
        private TransactionSignatureVerifyer _transactionSignatureVerifyer;
        private Mock<IAccountContainer> _accountContainerMock;
        private Mock<ITransaction> _transactionMock;
        private Mock<IAccount> _accountMock;
        private Mock<ICrypto> _cryptoMock;
        private Mock<IBlock> _lastBlock;

        [TestInitialize]
        public void TestInit()
        {
            var cryptoFactoryMock = new Mock<ICryptoFactory>();
            var blockRepositoryMock = new Mock<IBlockRepository>();
            _lastBlock = new Mock<IBlock>();
            _accountContainerMock = new Mock<IAccountContainer>();
            _accountMock = new Mock<IAccount>();
            _cryptoMock = new Mock<ICrypto>();
            _transactionMock = new Mock<ITransaction>();
            var bytes = new byte[160];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = 1;

            _accountContainerMock.Setup(ac => ac.GetAccount(It.IsAny<long>())).Returns(_accountMock.Object);
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock + 1);
            _lastBlock.SetupGet(b => b.Height).Returns(Constants.NQTBlock + 1);
            cryptoFactoryMock.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);
            _cryptoMock.Setup(
                c => c.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<bool>()))
                .Returns(true);
            _accountMock.Setup(a => a.SetAndVerifyPublicKey(It.IsAny<byte[]>(), It.IsAny<int>())).Returns(true);
            blockRepositoryMock.SetupGet(r => r.LastBlock).Returns(_lastBlock.Object);

            _transactionSignatureVerifyer = new TransactionSignatureVerifyer(_accountContainerMock.Object, cryptoFactoryMock.Object);
        }

        [TestMethod]
        public void VerifyTransactionShouldPass()
        {
            var actual = _transactionSignatureVerifyer.VerifyTransaction(_transactionMock.Object);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void VerifyTransactionShouldNotAcceptNullAccount()
        {
            _accountContainerMock.Setup(ac => ac.GetAccount(It.IsAny<long>())).Returns((IAccount) null);

            var actual = _transactionSignatureVerifyer.VerifyTransaction(_transactionMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyTransactionShouldNotAcceptNullTransactionSignature()
        {
            _transactionMock.SetupGet(t => t.Signature).Returns((byte[]) null);

            var actual = _transactionSignatureVerifyer.VerifyTransaction(_transactionMock.Object);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyTransactionShouldCallVerifyWithFalseCanonicalLinkWhenPublicKeyIsNotVerified()
        {
            _accountMock.Setup(a => a.SetAndVerifyPublicKey(It.IsAny<byte[]>(), It.IsAny<int>())).Returns(false);

            _transactionSignatureVerifyer.VerifyTransaction(_transactionMock.Object);

            _cryptoMock.Verify(c => c.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.Is<bool>(value => !value)));
        }

        [TestMethod]
        public void VerifyTransactionShouldCallVerifyWithFalseCanonicalLinkWhenHeightIsLessThanNQTBlock()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);
            _lastBlock.SetupGet(b => b.Height).Returns(Constants.NQTBlock - 1);

            _transactionSignatureVerifyer.VerifyTransaction(_transactionMock.Object);

            _cryptoMock.Verify(c => c.Verify(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.Is<bool>(value => !value)));
        }

        [TestMethod]
        public void VerifyTransactionShouldFetchBytesWithZeroSignature()
        {
            _transactionMock.SetupGet(t => t.Height).Returns(Constants.NQTBlock - 1);

            _transactionSignatureVerifyer.VerifyTransaction(_transactionMock.Object);

            _transactionMock.Verify(t => t.GetBytes(It.Is<bool>(b => b)));
        }
    }
}
