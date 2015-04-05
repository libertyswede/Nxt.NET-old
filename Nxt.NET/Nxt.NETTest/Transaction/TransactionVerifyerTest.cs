using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionVerifyerTest
    {
        private Mock<IConvert> _convertMock;
        private Mock<IBlockRepository> _blockRepositoryMock;
        private TransactionVerifyer _transactionVerifyer;
        private Mock<IBlock> _blockMock;
        private Mock<IBlock> _previousBlockMock;
        private Mock<ITransaction> _transactionMock;
        private Mock<ITransactionRepository> _transactionRepositoryMock;
        private byte[] _referencedTransactionFullHash;
        private Mock<ITransactionSignatureVerifyer> _transactionSignatureVerifyerMock;
        private Mock<ICrypto> _cryptoMock;
        private Mock<IUnconfirmedTransactionApplier> _unconfirmedTransactionApplierMock;

        [TestInitialize]
        public void TestInit()
        {
            _convertMock = new Mock<IConvert>();
            _blockRepositoryMock = new Mock<IBlockRepository>();
            _blockMock = new Mock<IBlock>();
            _previousBlockMock = new Mock<IBlock>();
            _transactionMock = new Mock<ITransaction>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _transactionSignatureVerifyerMock = new Mock<ITransactionSignatureVerifyer>();
            _cryptoMock = new Mock<ICrypto>();
            _unconfirmedTransactionApplierMock = new Mock<IUnconfirmedTransactionApplier>();
            var cryptoFactoryMock = new Mock<ICryptoFactory>();
            var configurationMock = new Mock<IConfiguration>();

            ObjectFactory.Configure(x => x.For<IUnconfirmedTransactionApplier>().Use(_unconfirmedTransactionApplierMock.Object));

            var payloadHash = new byte[] {3, 2, 1};
            _referencedTransactionFullHash = new byte[] { 1, 2, 3 };
            configurationMock.SetupGet(c => c.IsTestnet).Returns(false);
            cryptoFactoryMock.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);
            _cryptoMock.SetupGet(c => c.Hash).Returns(payloadHash);
            _blockRepositoryMock.SetupGet(r => r.LastBlock).Returns(_previousBlockMock.Object);
            _blockMock.Setup(b => b.Transactions).Returns(new List<ITransaction> { _transactionMock.Object });
            _blockMock.SetupGet(b => b.Timestamp).Returns(100);
            _blockMock.SetupGet(b => b.PayloadHash).Returns(payloadHash);
            _blockMock.SetupGet(b => b.TotalAmount).Returns(1*Constants.OneNxt);
            _blockMock.SetupGet(b => b.TotalFee).Returns(1*Constants.OneNxt);
            _convertMock.Setup(c => c.GetEpochTime()).Returns(100);
            _convertMock.Setup(c => c.FullHashToId(It.IsAny<byte[]>())).Returns(42);
            _convertMock.Setup(c => c.FullHashToId(It.Is<byte[]>(b => b.SequenceEqual(_referencedTransactionFullHash)))).Returns(43);
            _transactionMock.SetupGet(t => t.Id).Returns(42);
            _transactionMock.SetupGet(t => t.Timestamp).Returns(100);
            _transactionMock.Setup(t => t.GetExpiration()).Returns(100);
            _transactionMock.Setup(t => t.ReferencedTransactionFullHash).Returns(_referencedTransactionFullHash);
            _transactionMock.Setup(t => t.TransactionType).Returns(new OrdinaryPayment(configurationMock.Object));
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(1*Constants.OneNxt);
            _transactionMock.SetupGet(t => t.FeeNQT).Returns(1*Constants.OneNxt);
            _transactionRepositoryMock.Setup(r => r.HasTransaction(It.IsAny<long>())).ReturnsAsync(false);
            _transactionRepositoryMock.Setup(r => r.HasTransaction(It.Is<long>(id => id == 43))).ReturnsAsync(true);
            _transactionSignatureVerifyerMock.Setup(v => v.VerifyTransaction(It.IsAny<ITransaction>())).Returns(true);
            _unconfirmedTransactionApplierMock.Setup(a => a.ApplyUnconfirmedTransaction(It.IsAny<ITransaction>()))
                .Returns(true);

            _transactionVerifyer = new TransactionVerifyer(_convertMock.Object, _blockRepositoryMock.Object,
                _transactionRepositoryMock.Object, configurationMock.Object, _transactionSignatureVerifyerMock.Object,
                cryptoFactoryMock.Object);
        }

        [TestMethod]
        public async Task VerifyTransactionsShouldPass()
        {
            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptTransactionTimestampLaterThanCurrentTime()
        {
            _convertMock.Setup(c => c.GetEpochTime()).Returns(100);
            _transactionMock.SetupGet(t => t.Timestamp).Returns(200);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptTransactionTimestampLaterThanBlockTime()
        {
            _transactionMock.SetupGet(t => t.Timestamp).Returns(100);
            _convertMock.Setup(c => c.GetEpochTime()).Returns(50);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptExpiredTransaction()
        {
            _blockMock.SetupGet(b => b.Timestamp).Returns(100);
            _transactionMock.Setup(t => t.GetExpiration()).Returns(10);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptAlreadyExistingTransactionId()
        {
            _transactionMock.SetupGet(t => t.Id).Returns(42);
            _transactionRepositoryMock.Setup(r => r.HasTransaction(It.Is<long>(id => id == 42))).ReturnsAsync(true);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptTransactionIdZero()
        {
            _transactionMock.SetupGet(t => t.Id).Returns(0);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        public async Task VerifyTransactionsShouldPassPostFullHashBlock()
        {
            const string hashString = "010203";
            var previousTransactionMock = new Mock<ITransaction>();
            previousTransactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns((byte[]) null);
            _previousBlockMock.SetupGet(b => b.Height).Returns(Constants.ReferencedTransactionFullHashBlock + 1);
            _convertMock.Setup(c => c.ToHexString(It.Is<byte[]>(b => b.SequenceEqual(_referencedTransactionFullHash))))
                .Returns(hashString);
            _transactionRepositoryMock.Setup(r => r.GetTransactionByFullHash(It.Is<string>(id => id.Equals(hashString))))
                .ReturnsAsync(previousTransactionMock.Object);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptFaultyPreTransactionFullHash()
        {
            _transactionRepositoryMock.Setup(r => r.HasTransaction(It.Is<long>(id => id == 43))).ReturnsAsync(false);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptFaultyPostTransactionFullHash()
        {
            const string hashString = "010203";
            var previousTransactionMock = new Mock<ITransaction>();
            previousTransactionMock.SetupGet(t => t.ReferencedTransactionFullHash).Returns((byte[])null);
            previousTransactionMock.SetupGet(t => t.Timestamp).Returns(100 - (60*60*24*60) - 1);
            _previousBlockMock.SetupGet(b => b.Height).Returns(Constants.ReferencedTransactionFullHashBlock + 1);
            _convertMock.Setup(c => c.ToHexString(It.Is<byte[]>(b => b.SequenceEqual(_referencedTransactionFullHash))))
                .Returns(hashString);
            _transactionRepositoryMock.Setup(r => r.GetTransactionByFullHash(It.Is<string>(id => id.Equals(hashString))))
                .ReturnsAsync(previousTransactionMock.Object);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptFaultySignature()
        {
            _transactionSignatureVerifyerMock.Setup(v => v.VerifyTransaction(It.IsAny<ITransaction>())).Returns(false);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        public async Task VerifyTransactionsShouldCallValidateAttachment()
        {
            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);

            _transactionMock.Verify(t => t.ValidateAttachment());
        }

        [TestMethod]
        [ExpectedException(typeof(BlockNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptMismatchInTransactionAmountAndBlockAmount()
        {
            _transactionMock.SetupGet(t => t.AmountNQT).Returns(2*Constants.OneNxt);
            _blockMock.SetupGet(b => b.TotalAmount).Returns(1*Constants.OneNxt);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(BlockNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptMismatchInTransactionFeeAndBlockFee()
        {
            _transactionMock.SetupGet(t => t.FeeNQT).Returns(2 * Constants.OneNxt);
            _blockMock.SetupGet(b => b.TotalFee).Returns(1 * Constants.OneNxt);

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(BlockNotAcceptedException))]
        public async Task VerifyTransactionsShouldNotAcceptMismatchInPayloadHash()
        {
            _cryptoMock.SetupGet(c => c.Hash).Returns(new byte[] {3, 4, 5});
            _blockMock.SetupGet(b => b.PayloadHash).Returns(new byte[] {4, 5, 6});

            await _transactionVerifyer.VerifyTransactions(_blockMock.Object);
        }
    }
}
