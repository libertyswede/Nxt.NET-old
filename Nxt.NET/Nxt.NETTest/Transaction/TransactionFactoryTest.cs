using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NETTest.Transaction
{
    [TestClass]
    public class TransactionFactoryTest
    {
        private Mock<ICryptoFactory> _cryptoFactory;
        private Mock<ICrypto> _cryptoMock;
        private Mock<ITransactionTypeFactory> _transactionTypeFactoryMock;
        private Mock<IConvert> _convertMock;
        private TransactionFactory _transactionFactory;
        private Mock<ITransaction> _transactionMock;
        private Mock<ITransactionType> _transactionTypeMock;

        [TestInitialize]
        public void TestInit()
        {
            _cryptoFactory = new Mock<ICryptoFactory>();
            _cryptoMock = new Mock<ICrypto>();
            _transactionTypeFactoryMock = new Mock<ITransactionTypeFactory>();
            _transactionTypeMock = new Mock<ITransactionType>();
            _convertMock = new Mock<IConvert>();
            _transactionMock = new Mock<ITransaction>();

            ObjectFactory.Configure(x => x.For<ITransaction>().Use(_transactionMock.Object));
            _transactionMock.SetupAllProperties();
            _transactionTypeFactoryMock.Setup(tf => tf.FindTransactionType(It.IsAny<byte>(), It.IsAny<byte>()))
                .Returns(_transactionTypeMock.Object);
            _cryptoFactory.Setup(cf => cf.Create()).Returns(_cryptoMock.Object);

            _transactionFactory = new TransactionFactory(_cryptoFactory.Object, _transactionTypeFactoryMock.Object,
                _convertMock.Object);
        }

        [TestMethod]
        public void CreateFullShouldCreateTransaction()
        {
            byte type = 0;
            byte subType = 0;
            var timestamp = 100;
            short deadline = 1440;
            var senderPublicKey = new byte[] {1, 2, 3, 4, 5, 6, 7, 8};
            long recipientId = 42;
            var amount = 5*Constants.OneNxt;
            var fee = 1*Constants.OneNxt;
            var referencedTransactionFullHash = new byte[] {2, 3, 4, 5, 6, 7, 8, 9};
            var signature = new byte[] {3, 4, 5, 6, 7, 8, 9, 10, 11};
            var blockId = 43;
            var height = 44;
            var id = 45;
            var senderId = 46;
            var blockTimestamp = 105;
            var fullHash = new byte[]{4, 5, 6, 7, 8, 9, 10, 11, 12};
            var attachmentData = new byte[] {5, 6, 7, 8, 9, 10, 11, 12, 13};
            _cryptoMock.Setup(c => c.ComputeHash(It.IsAny<byte[]>())).Returns(fullHash);

            var actual = _transactionFactory.Create(type, subType, timestamp, deadline, senderPublicKey, recipientId, amount, fee,
                referencedTransactionFullHash, signature, blockId, height, id, senderId, blockTimestamp, fullHash,
                attachmentData);

            Assert.AreSame(_transactionMock.Object, actual);
        }
    }
}
