using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BaseTargetCalculatorTest
    {
        private BaseTargetCalculator _baseTargetCalculator;
        private Mock<IBlock> _currentBlockMock;
        private Mock<IBlock> _previousBlockMock;

        [TestInitialize]
        public void TestInit()
        {
            _previousBlockMock = new Mock<IBlock>();
            _currentBlockMock = new Mock<IBlock>();

            SetupBlockMock(_previousBlockMock, Constants.InitialBaseTarget, 0);
            SetupBlockMock(_currentBlockMock, Constants.InitialBaseTarget, 793);

            _baseTargetCalculator = new BaseTargetCalculator();
        }

        private static void SetupBlockMock(Mock<IBlock> blockMock, long baseTarget, int timestamp)
        {
            blockMock.SetupGet(b => b.BaseTarget).Returns(baseTarget);
            blockMock.SetupGet(b => b.Timestamp).Returns(timestamp);
        }

        [TestMethod]
        public void CalculateAndSetBaseTargetShouldSetInitialValuesForGenesisBlock()
        {
            _currentBlockMock.SetupGet(b => b.Id).Returns(Genesis.GenesisBlockId);
            _currentBlockMock.SetupGet(b => b.PreviousBlockId).Returns((long?) null);

            _baseTargetCalculator.CalculateAndSetBaseTarget(_previousBlockMock.Object, _currentBlockMock.Object);

            _currentBlockMock.VerifySet(b => b.BaseTarget = Constants.InitialBaseTarget);
            _currentBlockMock.VerifySet(b => b.CumulativeDifficulty = BigInteger.Zero);
        }

        // Simulation of recieving the second block after genesis block
        [TestMethod]
        public void CalculateAndSetBaseTargetShouldSetSetCorrectValuesForSecondBlock()
        {
            _baseTargetCalculator.CalculateAndSetBaseTarget(_previousBlockMock.Object, _currentBlockMock.Object);

            _currentBlockMock.VerifySet(b => b.BaseTarget = 307445734);
            _currentBlockMock.VerifySet(b => b.CumulativeDifficulty = new BigInteger(60000000109));
        }

        [TestMethod]
        public void CalculateNewBaseTargetShouldNotExceedMaxBaseTarget()
        {
            SetupBlockMock(_previousBlockMock, Constants.MaxBaseTarget, 100);
            SetupBlockMock(_currentBlockMock, Constants.MaxBaseTarget, 170);

            _baseTargetCalculator.CalculateAndSetBaseTarget(_previousBlockMock.Object, _currentBlockMock.Object);

            _currentBlockMock.VerifySet(b => b.BaseTarget = Constants.MaxBaseTarget);
        }

        [TestMethod]
        public void CalculateNewBaseTargetShouldNotDecreaseMoreThanHalf()
        {
            SetupBlockMock(_previousBlockMock, Constants.MaxBaseTarget, 100);
            SetupBlockMock(_currentBlockMock, Constants.MaxBaseTarget, 110);

            _baseTargetCalculator.CalculateAndSetBaseTarget(_previousBlockMock.Object, _currentBlockMock.Object);

            _currentBlockMock.VerifySet(b => b.BaseTarget = (Constants.MaxBaseTarget / 2));
        }

        [TestMethod]
        public void CalculateNewBaseTargetShouldSetBaseTargetToOneWhenBlocksHaveSameTimestamp()
        {
            SetupBlockMock(_previousBlockMock, 1, 100);
            SetupBlockMock(_currentBlockMock, 1, 100);

            _baseTargetCalculator.CalculateAndSetBaseTarget(_previousBlockMock.Object, _currentBlockMock.Object);

            _currentBlockMock.VerifySet(b => b.BaseTarget = 1);
        }
    }
}
