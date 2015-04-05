using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Nxt.NET.Transaction;

namespace Nxt.NET.Block
{
    public interface IBlockVerifyer
    {
        Task VerifyBlock(IBlock block);
        void VerifyGenerationSignature(IBlock block, IBlock previousLastBlock);
        void VerifyBlockSignature(IBlock block);
        void VerifyVersion(IBlock block, int currentHeight);
    }

    public class BlockVerifyer : IBlockVerifyer
    {
        private readonly IBlockRepository _blockRepository;
        private readonly IConvert _convert;
        private readonly IConfiguration _configuration;
        private readonly ITransactionsChecksumCalculator _transactionsChecksumCalculator;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IBlockGenerationSignatureVerifyer _blockGenerationSignatureVerifyer;
        private readonly IBlockSignatureVerifyer _blockSignatureVerifyer;

        public BlockVerifyer(IBlockRepository blockRepository, IConvert convert, IConfiguration configuration,
            ITransactionsChecksumCalculator transactionsChecksumCalculator,
            IBlockGenerationSignatureVerifyer blockGenerationSignatureVerifyer,
            IBlockSignatureVerifyer blockSignatureVerifyer)
        {
            _blockRepository = blockRepository;
            _convert = convert;
            _configuration = configuration;
            _transactionsChecksumCalculator = transactionsChecksumCalculator;
            _blockGenerationSignatureVerifyer = blockGenerationSignatureVerifyer;
            _blockSignatureVerifyer = blockSignatureVerifyer;
        }

        public async Task VerifyBlock(IBlock block)
        {
            var previousLastBlock = _blockRepository.LastBlock;
            var currentTime = _convert.GetEpochTime();

            VerifyPreviousBlockId(block, previousLastBlock);
            VerifyVersion(block, previousLastBlock.Height);
            await VerifySpecialBlock(Constants.TransparentForgingBlock, previousLastBlock, Constants.ChecksumTransparentForging);
            await VerifySpecialBlock(Constants.GetNQTBlockHeight(_configuration), previousLastBlock, Constants.GetNqtBlockChecksum(_configuration));
            VerifyPreviousBlockHash(block, previousLastBlock);
            VerifyTimestamp(block, currentTime, previousLastBlock);
            await VerifyBlockId(block);
            VerifyGenerationSignature(block, previousLastBlock);
            VerifyBlockSignature(block);
        }

        public void VerifyVersion(IBlock block, int currentHeight)
        {
            bool isVerified;
            if (currentHeight < Constants.TransparentForgingBlock)
                isVerified = block.Version == 1;
            else if (currentHeight < Constants.GetNQTBlockHeight(_configuration))
                isVerified = block.Version == 2;
            else
                isVerified = block.Version == 3;
            if (!isVerified)
                throw new BlockNotAcceptedException("Invalid version " + block.Version);
        }

        public void VerifyGenerationSignature(IBlock block, IBlock previousLastBlock)
        {
            var verified = _blockGenerationSignatureVerifyer.VerifyGenerationSignature(block, previousLastBlock);
            if (!verified)
                throw new BlockNotAcceptedException("Generation signature verification failed");
        }

        public void VerifyBlockSignature(IBlock block)
        {
            var verified = _blockSignatureVerifyer.VerifyBlockSignature(block);
            if (!verified)
                throw new BlockNotAcceptedException("Block signature verification failed");
        }

        private static void VerifyPreviousBlockId(IBlock block, IBlock previousLastBlock)
        {
            if (!previousLastBlock.Id.Equals(block.PreviousBlockId))
            {
                throw new BlockOutOfOrderException("Previous block id doesn't match");
            }
        }

        private async Task VerifySpecialBlock(int blockHeight, IBlock previousLastBlock, IEnumerable<byte> correctChecksum)
        {
            if (previousLastBlock.Height == blockHeight)
            {
                var checksum = await _transactionsChecksumCalculator.CalculateAllTransactionsChecksum();
                if (!checksum.SequenceEqual(correctChecksum))
                {
                    Logger.Error("Checksum failed at block " + blockHeight);
                    throw new BlockNotAcceptedException("Checksum failed");
                }
                Logger.Info("Checksum passed at block " + blockHeight);
            }
        }

        private static void VerifyPreviousBlockHash(IBlock block, IBlock previousLastBlock)
        {
            if (block.Version != 1 && !previousLastBlock.BlockHash.SequenceEqual(block.PreviousBlockHash))
                throw new BlockNotAcceptedException("Previous block hash doesn't match");
        }

        private static void VerifyTimestamp(IBlock block, int currentTime, IBlock previousLastBlock)
        {
            if (block.Timestamp > currentTime + 15 || block.Timestamp <= previousLastBlock.Timestamp)
            {
                throw new BlockOutOfOrderException(
                    string.Format("Invalid timestamp: {0} current time is {1}, previous block timestamp is {2}",
                        block.Timestamp, currentTime, previousLastBlock.Timestamp));
            }
        }

        private async Task VerifyBlockId(IBlock block)
        {
            if (block.Id == 0 || await _blockRepository.HasBlock(block.Id))
                throw new BlockNotAcceptedException("Duplicate block or invalid id");
        }
    }
}
