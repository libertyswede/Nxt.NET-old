using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Peer;
using Nxt.NET.Transaction;
using StructureMap;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockchainProcessorTest
    {

        private IBlockchainProcessor _blockchainProcessor;
        private Mock<ITransactionProcessor> _transactionProcessorMock;
        private Mock<IBlockVerifyer> _blockVerifyerMock;
        private Mock<IBlockRepository> _repositoryMock;
        private Mock<IPeerContainer> _peersMock;
        private Mock<IBlockFactory> _blockFactoryMock;
        private Mock<IGenesis> _genesisMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IAccountContainer> _accountContainerMock;
        private Mock<IBaseTargetCalculator> _baseTargetCalculatorMock;

        [TestInitialize]
        public void TestInit()
        {
            _repositoryMock = new Mock<IBlockRepository>();
            _transactionProcessorMock = new Mock<ITransactionProcessor>();
            _peersMock = new Mock<IPeerContainer>();
            _blockVerifyerMock = new Mock<IBlockVerifyer>();
            _blockFactoryMock = new Mock<IBlockFactory>();
            _genesisMock = new Mock<IGenesis>();
            _configurationMock = new Mock<IConfiguration>();
            _accountContainerMock = new Mock<IAccountContainer>();
            _baseTargetCalculatorMock = new Mock<IBaseTargetCalculator>();

            ObjectFactory.Configure(x => x.For<IBlockFactory>().Use(_blockFactoryMock.Object));

            _blockchainProcessor = new BlockchainProcessor(_repositoryMock.Object, _transactionProcessorMock.Object,
                _peersMock.Object, _blockVerifyerMock.Object, _genesisMock.Object,
                _configurationMock.Object, _accountContainerMock.Object,
                _baseTargetCalculatorMock.Object);
        }

        [TestMethod]
        public async Task AddGenesisBlockShouldNotAddIfAlreadyExists()
        {
            _repositoryMock.Setup(r => r.HasBlock(It.Is<long>(id => id == Genesis.GenesisBlockId))).ReturnsAsync(true);

            await _blockchainProcessor.AddGenesisBlockIfNeeded();

            _repositoryMock.Verify(r => r.AddBlock(It.IsAny<NET.Block.Block>()), Times.Never);
        }

        [TestMethod]
        public async Task AddGenesisBlockShouldAddGenesisBlock()
        {
            var expected = new Mock<IBlock>();
            _repositoryMock.Setup(r => r.HasBlock(It.Is<long>(id => id == Genesis.GenesisBlockId))).ReturnsAsync(false);
            _genesisMock.Setup(g => g.GetGenesisBlock(It.IsAny<IBlockFactory>(), It.IsAny<IBaseTargetCalculator>(), It.IsAny<IList<ITransaction>>()))
                .Returns(expected.Object);
            expected.Setup(b => b.Transactions).Returns(new List<ITransaction>());
            var accountMock = new Mock<IAccount>();
            accountMock.SetupGet(a => a.Balance).Returns(new Mock<IAccountBalance>().Object);
            _accountContainerMock.Setup(ac => ac.GetOrAddAccount(It.IsAny<long>())).Returns(accountMock.Object);

            await _blockchainProcessor.AddGenesisBlockIfNeeded();

            _repositoryMock.Verify(r => r.AddBlock(It.Is<IBlock>(b => b == expected.Object)), Times.Once);
            _repositoryMock.VerifySet(r => r.LastBlock = expected.Object);
        }

        [TestMethod]
        public async Task PushBlockShouldVerifyBlock()
        {
            var block = CreatePushBlock();

            await _blockchainProcessor.PushBlock(block.Object);

            _blockVerifyerMock.Verify(bv => bv.VerifyBlock(It.Is<IBlock>(b => b == block.Object)));
            _transactionProcessorMock.Verify(tp => tp.VerifyTransactions(It.Is<IBlock>(b => b == block.Object)));
        }

        [TestMethod]
        public async Task PushBlockShouldAddBlock()
        {
            var block = CreatePushBlock();

            await _blockchainProcessor.PushBlock(block.Object);

            _repositoryMock.VerifySet(r => r.LastBlock = block.Object);
            _repositoryMock.Verify(r => r.AddBlock(It.Is<IBlock>(b => b == block.Object)));
            block.VerifySet(b => b.Height = 0);
        }

        [TestMethod]
        public async Task PushBlockShouldSetCorrectBlockPropertiesWhenNotGenesis()
        {
            var block = CreatePushBlock();
            var lastBlockMock = new Mock<IBlock>();
            lastBlockMock.SetupGet(b => b.Id).Returns(42);
            lastBlockMock.SetupGet(b => b.Height).Returns(43);
            _repositoryMock.SetupGet(r => r.LastBlock).Returns(lastBlockMock.Object);

            await _blockchainProcessor.PushBlock(block.Object);

            block.VerifySet(b => b.PreviousBlockId = 42);
            block.VerifySet(b => b.Height = 44);
            _baseTargetCalculatorMock.Verify(c => c.CalculateAndSetBaseTarget(
                It.Is<IBlock>(b => b == lastBlockMock.Object), 
                It.Is<IBlock>(b => b == block.Object)));
        }

        [TestMethod]
        public async Task PushBlockShouldConnectTransactionsToBlock()
        {
            var block = CreatePushBlock();
            block.SetupGet(b => b.Id).Returns(42);
            block.SetupGet(b => b.Height).Returns(43);
            block.SetupGet(b => b.Timestamp).Returns(44);
            var transactionMock = new Mock<ITransaction>();
            block.SetupGet(b => b.Transactions)
                .Returns(new ReadOnlyCollection<ITransaction>(new[] {transactionMock.Object}));

            await _blockchainProcessor.PushBlock(block.Object);

            transactionMock.VerifySet(t => t.BlockId = 42);
            transactionMock.VerifySet(t => t.Height = 43);
            transactionMock.VerifySet(t => t.BlockTimestamp = 44);
        }

        [TestMethod]
        public void ApplyBlockShouldFireEvents()
        {
            var block = CreatePushBlock();
            var beforeApplyBlockCalled = false;
            var afterApplyBlockCalled = false;
            _blockchainProcessor.BeforeApplyBlock += (processor, args) => beforeApplyBlockCalled = true;
            _blockchainProcessor.AfterApplyBlock += (processor, args) => afterApplyBlockCalled = true;

            _blockchainProcessor.ApplyBlock(block.Object);

            Assert.IsTrue(beforeApplyBlockCalled && afterApplyBlockCalled);
        }

        [TestMethod]
        public void ApplyBlockShouldApplyGeneratorAccountPublicKey()
        {
            var blockMock = new Mock<IBlock>();
            var account = new Mock<IAccount>();
            var accountBalance = new Mock<IAccountBalance>();
            account.SetupGet(a => a.Balance).Returns(accountBalance.Object);
            blockMock.SetupGet(b => b.Transactions).Returns(new ReadOnlyCollection<ITransaction>(new List<ITransaction>()));
            blockMock.SetupGet(b => b.GeneratorPublicKey).Returns(new byte[] { 1, 2, 3 });
            blockMock.SetupGet(b => b.Height).Returns(42);
            _accountContainerMock.Setup(ac => ac.GetOrAddAccount(It.IsAny<long>())).Returns(account.Object);
            blockMock.SetupGet(b => b.TotalFee).Returns(7);

            _blockchainProcessor.ApplyBlock(blockMock.Object);

            account.Verify(a => a.ApplyPublicKey(
                It.Is<byte[]>(b => b.SequenceEqual(new byte[]{1, 2, 3})), 
                It.Is<int>(i => i == 42)));
            accountBalance.Verify(ab => ab.AddToBalanceAndUnconfirmedBalanceNQT(It.Is<long>(a => a == 7)));
            accountBalance.Verify(ab => ab.AddToForgedBalanceNQT(It.Is<long>(a => a == 7)));
            _transactionProcessorMock.Verify(tp => tp.ApplyTransactions(It.Is<IBlock>(b => b == blockMock.Object)));
        }

        private Mock<IBlock> CreatePushBlock()
        {
            var block = new Mock<IBlock>();
            var account = new Mock<IAccount>();
            account.SetupGet(a => a.Balance).Returns(new Mock<IAccountBalance>().Object);
            block.SetupGet(b => b.Transactions).Returns(new ReadOnlyCollection<ITransaction>(new List<ITransaction>()));
            _accountContainerMock.Setup(ac => ac.GetOrAddAccount(It.IsAny<long>())).Returns(account.Object);
            return block;
        }
    }
}
