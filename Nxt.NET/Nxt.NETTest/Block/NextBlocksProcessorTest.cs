using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Request;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class NextBlocksProcessorTest
    {
        private Mock<IBlockRepository> _repositoryMock;
        private Mock<IBlockchainProcessor> _blockchainProcessorMock;
        private Mock<IBlock> _genesisBlockMock;
        private NextBlocksProcessor _nextBlocksProcessor;
        private PeerTestHelper _peerHelper;
        private Mock<IBlock> _secondBlock;
        private Mock<IBlock> _thirdBlock;
        private const long SecondBlockId = 6556228577102711328;
        private const long ThirdBlockId = 3705873229391760257;

        [TestInitialize]
        public void TestInit()
        {
            _blockchainProcessorMock = new Mock<IBlockchainProcessor>();
            _repositoryMock = new Mock<IBlockRepository>();

            SetupPeerHelper();
            SetupBlocks();
            SetupCumulativeDifficulty();
            
            _nextBlocksProcessor = new NextBlocksProcessor(_blockchainProcessorMock.Object, _repositoryMock.Object);
        }

        [TestMethod]
        public async Task GetNextBlocksShouldCallVerify()
        {
            SetupRepository(_genesisBlockMock.Object);
            _peerHelper.NextBlockIdsMock.SetupGet(n => n.NextBlockIds)
                .Returns(new ReadOnlyCollection<long>(new List<long> { SecondBlockId }));

            await _nextBlocksProcessor.GetNextBlocks(_peerHelper.PeerMock.Object);

            _blockchainProcessorMock.Verify(bp => bp.PushBlock(It.Is<IBlock>(b => b == _secondBlock.Object)), Times.Once);
        }

        [TestMethod]
        public async Task WhenDbHasContentsShouldCallVerify()
        {
            SetupRepository(_secondBlock.Object);
            _peerHelper.NextBlockIdsMock.SetupGet(n => n.NextBlockIds)
                .Returns(new ReadOnlyCollection<long>(new[] {ThirdBlockId}));
            _peerHelper.MilestoneBlockIdsMock.SetupGet(mbi => mbi.MilestonBlockIds)
                .Returns(new ReadOnlyCollection<long>(new[] {SecondBlockId}));

            await _nextBlocksProcessor.GetNextBlocks(_peerHelper.PeerMock.Object);

            _blockchainProcessorMock.Verify(bp => bp.PushBlock(It.Is<IBlock>(b => b == _thirdBlock.Object)), Times.Once);
        }

        private void SetupRepository(IBlock block)
        {
            _repositoryMock.Setup(r => r.LastBlock).Returns(block);
            _repositoryMock.Setup(r => r.GetBlock(It.Is<long>(id => id == block.Id)))
                .ReturnsAsync(block);
            _repositoryMock.Setup(r => r.HasBlock(block.Id)).ReturnsAsync(true);
        }

        private void SetupPeerHelper()
        {
            _peerHelper = new PeerTestHelper();
            _peerHelper.PeerMock.SetupGet(p => p.IsConnected).Returns(true);
            _peerHelper.PeerMock.Setup(p => p.SendRequest(It.IsAny<GetNextBlocksRequest>()))
                .ReturnsAsync(null);
        }

        private void SetupBlocks()
        {
            _genesisBlockMock = new Mock<IBlock>();
            _genesisBlockMock.SetupGet(b => b.Id).Returns(Genesis.GenesisBlockId);
            _genesisBlockMock.SetupGet(b => b.CumulativeDifficulty).Returns(new BigInteger());
            _genesisBlockMock.SetupGet(b => b.Height).Returns(0);

            _secondBlock = new Mock<IBlock>();
            _secondBlock.SetupGet(b => b.PreviousBlockId).Returns(Genesis.GenesisBlockId);
            _secondBlock.SetupGet(b => b.Id).Returns(SecondBlockId);
            _peerHelper.SetupNextBlocks(Genesis.GenesisBlockId, new List<IBlock> { _secondBlock.Object });

            _thirdBlock = new Mock<IBlock>();
            _thirdBlock.SetupGet(b => b.PreviousBlockId).Returns(SecondBlockId);
            _thirdBlock.SetupGet(b => b.Id).Returns(ThirdBlockId);
            _peerHelper.SetupNextBlocks(SecondBlockId, new List<IBlock> { _thirdBlock.Object });
        }


        private void SetupCumulativeDifficulty()
        {
            _peerHelper.CumulativeDifficultyMock.SetupGet(r => r.CumulativeDifficulty)
                .Returns(BigInteger.Parse("5349203914610810"));
            _peerHelper.CumulativeDifficultyMock.SetupGet(r => r.BlockchainHeight).Returns(159344);
        }
    }
}
