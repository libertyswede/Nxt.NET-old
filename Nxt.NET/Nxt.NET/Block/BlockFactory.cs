using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using NLog;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction;
using StructureMap;

namespace Nxt.NET.Block
{
    public interface IBlockFactory
    {
        IBlock Create(int version, int timestamp, long? previousBlockId, long totalAmount, long totalFee,
            int payloadLength, byte[] payloadHash,
            byte[] generatorPublicKey, byte[] generationSignature, byte[] blockSignature, byte[] previousBlockHash,
            IList<ITransaction> transactions);

        IBlock Create(int version, int timestamp, long? previousBlockId, long totalAmount, long totalFee,
            int payloadLength, byte[] payloadHash, byte[] generatorPublicKey, byte[] generationSignature,
            byte[] blockSignature, byte[] previousBlockHash, ReadOnlyCollection<ITransaction> transactions,
            BigInteger cumulativeDifficulty, long baseTarget, long? nextBlockId, int height, long blockId,
            long generatorId, long rowId);
    }

    public class BlockFactory : IBlockFactory
    {
        private readonly ICryptoFactory _cryptoFactory;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BlockFactory(ICryptoFactory cryptoFactory)
        {
            _cryptoFactory = cryptoFactory;
        }

        public IBlock Create(int version, int timestamp, long? previousBlockId, long totalAmount, long totalFee, int payloadLength,
            byte[] payloadHash, byte[] generatorPublicKey, byte[] generationSignature, byte[] blockSignature,
            byte[] previousBlockHash, IList<ITransaction> transactions)
        {
            VerifyTransactionCount(transactions);
            VerifyPayloadLength(payloadLength);
            UpdatePreviousBlockHashIfNeeded(ref previousBlockHash, version);

            var block = ObjectFactory.GetInstance<IBlock>();
            block.Version = version;
            block.Timestamp = timestamp;
            block.PreviousBlockId = previousBlockId;
            block.TotalAmount = totalAmount;
            block.TotalFee = totalFee;
            block.PayloadLength = payloadLength;
            block.GeneratorPublicKey = generatorPublicKey;
            block.GenerationSignature = generationSignature;
            block.BlockSignature = blockSignature;
            block.PreviousBlockHash = previousBlockHash;
            block.CumulativeDifficulty = BigInteger.Zero;
            block.BaseTarget = Constants.InitialBaseTarget;

            block.Transactions = CreateOrderedTransactionCollection(transactions);
            block.PayloadHash = UpdatePayloadHashIfNeeded(payloadHash, block);
            block.BlockHash = GenerateBlockHash(block);
            block.Id = GenerateBlockId(block);
            block.GeneratorId = GenerateGeneratorId(block);

            return block;
        }

        public IBlock Create(int version, int timestamp, long? previousBlockId, long totalAmount, long totalFee, int payloadLength,
            byte[] payloadHash, byte[] generatorPublicKey, byte[] generationSignature, byte[] blockSignature,
            byte[] previousBlockHash, ReadOnlyCollection<ITransaction> transactions, BigInteger cumulativeDifficulty, long baseTarget,
            long? nextBlockId, int height, long blockId, long generatorId, long rowId)
        {
            var block = ObjectFactory.GetInstance<IBlock>();
            block.Version = version;
            block.Timestamp = timestamp;
            block.PreviousBlockId = previousBlockId;
            block.TotalAmount = totalAmount;
            block.TotalFee = totalFee;
            block.PayloadLength = payloadLength;
            block.PayloadHash = payloadHash;
            block.GeneratorPublicKey = generatorPublicKey;
            block.GenerationSignature = generationSignature;
            block.BlockSignature = blockSignature;
            block.PreviousBlockHash = previousBlockHash;
            block.Transactions = transactions;
            block.CumulativeDifficulty = cumulativeDifficulty;
            block.BaseTarget = baseTarget;
            block.NextBlockId = nextBlockId;
            block.Height = height;
            block.Id = blockId;
            block.GeneratorId = generatorId;
            block.RowId = rowId;

            block.BlockHash = GenerateBlockHash(block);

            return block;
        }

        private static ReadOnlyCollection<ITransaction> CreateOrderedTransactionCollection(IEnumerable<ITransaction> transactions)
        {
            return new ReadOnlyCollection<ITransaction>(transactions.OrderBy(tx => tx.Id).ToList());
        }

        private static void UpdatePreviousBlockHashIfNeeded(ref byte[] previousBlockHash, int version)
        {
            if (version == 1)
                previousBlockHash = null;
        }

        private byte[] UpdatePayloadHashIfNeeded(byte[] payloadHash, IBlock block)
        {
            if (payloadHash == null || payloadHash.Length == 0)
                payloadHash = GenerateTransactionsHash(block);
            return payloadHash;
        }

        private static void VerifyPayloadLength(int payloadLength)
        {
            if (payloadLength > Constants.MaxPayloadLength)
                throw new NxtException("Attempted to create a block with payloadLength " + payloadLength);
        }

        private static void VerifyTransactionCount(ICollection<ITransaction> transactions)
        {
            if (transactions.Count > Constants.MaxNumberOfTransactions)
                throw new NxtException(string.Format("Attempted to create a block with {0} transactions", transactions.Count));
        }

        private byte[] GenerateTransactionsHash(IBlock block)
        {
            var crypto = _cryptoFactory.Create();
            var transactions = block.Transactions.ToList();

            for (var i = 0; i < transactions.Count - 1; i++)
                crypto.TransformBlock(transactions[i].GetBytes());
            crypto.TransformFinalBlock(transactions[transactions.Count - 1].GetBytes());
            return crypto.Hash;
        }

        private long GenerateGeneratorId(IBlock block)
        {
            var crypto = _cryptoFactory.Create();
            var hash = crypto.ComputeHash(block.GeneratorPublicKey);
            return GenerateIdFromHash(hash);
        }

        private static long GenerateBlockId(IBlock block)
        {
            if (block.BlockSignature == null || block.BlockSignature.Length == 0)
            {
                throw new NxtException("Block is not signed yet");
            }
            var id = GenerateIdFromHash(block.BlockHash);
            block.Transactions.ToList().ForEach(t => t.BlockId = id);
            Logger.Debug(string.Format("Block Id:{0}, Hash bytes:{1}", id,
                block.BlockHash.Aggregate(string.Empty, (current, b) => current + (b + ", "))));

            return id;
        }

        private static long GenerateIdFromHash(IList<byte> hash)
        {
            var idHash = new[] { hash[0], hash[1], hash[2], hash[3], hash[4], hash[5], hash[6], hash[7] };
            var big = new BigInteger(idHash);
            return (long)big;
        }

        private byte[] GenerateBlockHash(IBlock block)
        {
            return _cryptoFactory.Create().ComputeHash(block.GetBytes());
        }
    }
}
