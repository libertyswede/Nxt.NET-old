using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockVerifyerTest
    {
        private Mock<IBlockRepository> _repositoryMock;
        private BlockVerifyer _blockVerify;
        private Mock<IBlock> _blockMock;
        private Mock<IBlock> _lastBlockMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IBlockGenerationSignatureVerifyer> _blockGenerationSignatureVerifyer;
        private Mock<IBlockSignatureVerifyer> _blockSignatureVerifyer;

        [TestInitialize]
        public void TestInit()
        {
            _repositoryMock = new Mock<IBlockRepository>();
            _blockMock = new Mock<IBlock>();
            _lastBlockMock = new Mock<IBlock>();
            _configurationMock = new Mock<IConfiguration>();
            _blockGenerationSignatureVerifyer = new Mock<IBlockGenerationSignatureVerifyer>();
            _blockSignatureVerifyer = new Mock<IBlockSignatureVerifyer>();
            var transactionsChecksumCalculatorMock = new Mock<ITransactionsChecksumCalculator>();
            var convert = new Mock<IConvert>();

            _blockGenerationSignatureVerifyer.Setup(
                v => v.VerifyGenerationSignature(It.IsAny<IBlock>(), It.IsAny<IBlock>())).Returns(true);
            transactionsChecksumCalculatorMock.Setup(tcc => tcc.CalculateAllTransactionsChecksum())
                .ReturnsAsync(new byte[] {1, 2, 3});
            _configurationMock.SetupGet(c => c.IsTestnet).Returns(false);
            _repositoryMock.Setup(r => r.LastBlock).Returns(_lastBlockMock.Object);
            _repositoryMock.Setup(r => r.HasBlock(It.Is<long>(id => id == 1))).ReturnsAsync(false);
            convert.Setup(c => c.GetEpochTime()).Returns(10);
            
            _blockMock.SetupGet(b => b.PreviousBlockId).Returns(1);
            _blockMock.SetupGet(b => b.Timestamp).Returns(10);
            _blockMock.SetupGet(b => b.Version).Returns(2);
            _blockMock.SetupGet(b => b.PreviousBlockHash).Returns(new byte[] { 1, 2, 3 });
            _blockMock.SetupGet(b => b.Id).Returns(1);

            _lastBlockMock.SetupGet(b => b.Id).Returns(1);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock + 1);
            _lastBlockMock.SetupGet(b => b.BlockHash).Returns(new byte[] { 1, 2, 3 });
            _lastBlockMock.SetupGet(b => b.Timestamp).Returns(5);

            _blockVerify = new BlockVerifyer(_repositoryMock.Object, convert.Object, _configurationMock.Object,
                transactionsChecksumCalculatorMock.Object, _blockGenerationSignatureVerifyer.Object,
                _blockSignatureVerifyer.Object);
        }

        [ExpectedException(typeof(BlockOutOfOrderException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptMismatchingIds()
        {
            _blockMock.SetupGet(b => b.PreviousBlockId).Returns(2);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptMismatchingVersionForTransparentForgingBlockHeight()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock - 1);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptMismatchingVersionForNQTBlockHeight()
        {
            _blockMock.SetupGet(b => b.Version).Returns(3);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.GetNQTBlockHeight(_configurationMock.Object) - 1);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptMismatchingVersionForLatestHeight()
        {
            _blockMock.SetupGet(b => b.Version).Returns(4);
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.GetNQTBlockHeight(_configurationMock.Object) + 1);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptMismatchingHashes()
        {
            _blockMock.SetupGet(b => b.PreviousBlockHash).Returns(new byte[] {1, 2, 3});
            _lastBlockMock.SetupGet(b => b.BlockHash).Returns(new byte[] {3, 2, 1});

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockOutOfOrderException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptTooEarlyTimestamp()
        {
            _blockMock.SetupGet(b => b.Timestamp).Returns(1);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockOutOfOrderException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptTooLateTimestamp()
        {
            _blockMock.SetupGet(b => b.Timestamp).Returns(50);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptZeroBlockId()
        {
            _blockMock.SetupGet(b => b.Id).Returns(0);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptBlockExistsInRepository()
        {
            _repositoryMock.Setup(r => r.HasBlock(It.Is<long>(id => id == 1))).ReturnsAsync(true);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptFaultyGenerationSignature()
        {
            _blockGenerationSignatureVerifyer.Setup(
                v => v.VerifyGenerationSignature(It.IsAny<IBlock>(), It.IsAny<IBlock>())).Returns(false);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptFaultyBlockSignature()
        {
            await _blockVerify.VerifyBlock(_blockMock.Object);
        }

        [ExpectedException(typeof(BlockNotAcceptedException))]
        [TestMethod]
        public async Task VerifyBlockShouldNotAcceptFaultyTransactionsChecksumForTransparentForgingBlock()
        {
            _lastBlockMock.SetupGet(b => b.Height).Returns(Constants.TransparentForgingBlock);

            await _blockVerify.VerifyBlock(_blockMock.Object);
        }
    }
}
