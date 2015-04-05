using System.Numerics;

namespace Nxt.NET.Block
{
    public interface IBaseTargetCalculator
    {
        void CalculateAndSetBaseTarget(IBlock previousBlock, IBlock currentBlock);
    }

    public class BaseTargetCalculator : IBaseTargetCalculator
    {
        private static readonly BigInteger Two64 = BigInteger.Parse("18446744073709551616");

        public void CalculateAndSetBaseTarget(IBlock previousBlock, IBlock currentBlock)
        {
            if (IsGenesisBlock(currentBlock))
            {
                SetGenesisBaseTarget(currentBlock);
            }
            else
            {
                var newBaseTarget = CalculateNewBaseTarget(previousBlock, currentBlock);
                currentBlock.BaseTarget = newBaseTarget;
                var newCumulativeDifficulty = BigInteger.Add(previousBlock.CumulativeDifficulty,
                    BigInteger.Divide(Two64, newBaseTarget));
                currentBlock.CumulativeDifficulty = newCumulativeDifficulty;
            }
        }

        private static bool IsGenesisBlock(IBlock currentBlock)
        {
            return currentBlock.Id == Genesis.GenesisBlockId && currentBlock.PreviousBlockId == null;
        }

        private static void SetGenesisBaseTarget(IBlock currentBlock)
        {
            currentBlock.BaseTarget = Constants.InitialBaseTarget;
            currentBlock.CumulativeDifficulty = BigInteger.Zero;
        }

        private static long CalculateNewBaseTarget(IBlock previousBlock, IBlock currentBlock)
        {
            var curBaseTarget = previousBlock.BaseTarget;
            var timestampDiff = currentBlock.Timestamp - previousBlock.Timestamp;
            var newBaseTarget = (long) BigInteger.Divide(BigInteger.Multiply(curBaseTarget, timestampDiff), 60);

            if (newBaseTarget < 0 || newBaseTarget > Constants.MaxBaseTarget)
            {
                newBaseTarget = Constants.MaxBaseTarget;
            }
            if (newBaseTarget < curBaseTarget/2)
            {
                newBaseTarget = curBaseTarget/2;
            }
            if (newBaseTarget == 0)
            {
                newBaseTarget = 1;
            }
            var twofoldCurBaseTarget = curBaseTarget*2;
            if (twofoldCurBaseTarget < 0)
            {
                twofoldCurBaseTarget = Constants.MaxBaseTarget;
            }
            if (newBaseTarget > twofoldCurBaseTarget)
            {
                newBaseTarget = twofoldCurBaseTarget;
            }
            return newBaseTarget;
        }
    }
}
