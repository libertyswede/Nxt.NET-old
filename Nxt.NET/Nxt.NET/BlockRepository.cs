using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nxt.NET.Block;
using Nxt.NET.Transaction;

namespace Nxt.NET
{
    public interface IBlockRepository
    {
        IBlock LastBlock { get; set; }
        Task<bool> HasBlock(long id);
        Task<IBlock> GetBlock(long id);
        Task<IList<IBlock>> GetAllBlocks();
        Task AddBlock(IBlock block);
        
    }

    public class BlockRepository : RepositoryBase, IBlockRepository
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBlockFactory _blockFactory;
        public IBlock LastBlock { get; set; }

        public BlockRepository(IDbController dbController, ITransactionRepository transactionRepository, IBlockFactory blockFactory)
            :base(dbController)
        {
            _transactionRepository = transactionRepository;
            _blockFactory = blockFactory;
        }

        public async Task<bool> HasBlock(long id)
        {
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1 FROM block WHERE id = @id";
                    cmd.Parameters.Add(new SQLiteParameter("@id") { Value = id });
                    var peerReader = await cmd.ExecuteScalarAsync();
                    return peerReader != null;
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read block from local database", e);
                return false;
            }
        }

        public async Task<IBlock> GetBlock(long id)
        {
            IBlock block = null;
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT rowid, id, version, timestamp, previous_block_id, total_amount, total_fee, " +
                        "payload_length, generator_public_key, previous_block_hash, cumulative_difficulty, base_target, " +
                        "next_block_id, height, generation_signature, block_signature, payload_hash, generator_id " +
                        "FROM block WHERE id = @id";

                    cmd.Parameters.Add(new SQLiteParameter("@id") { Value = id });
                    var peerReader = await cmd.ExecuteReaderAsync();
                    if (await peerReader.ReadAsync())
                    {
                        block = await GetBlock(peerReader);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read block from local database", e);
            }
            return block;
        }

        public async Task<IList<IBlock>> GetAllBlocks()
        {
            IList<IBlock> blocks = new List<IBlock>();
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    var allTransactions = await _transactionRepository.GetAllTransactions();
                    var groupedTransactions = allTransactions.GroupBy(t => t.BlockId).ToDictionary(g => g.Key, g => g.ToList());

                    cmd.CommandText =
                        "SELECT rowid, id, version, timestamp, previous_block_id, total_amount, total_fee, " +
                        "payload_length, generator_public_key, previous_block_hash, cumulative_difficulty, base_target, " +
                        "next_block_id, height, generation_signature, block_signature, payload_hash, generator_id " +
                        "FROM block ORDER BY rowid ASC";

                    var peerReader = await cmd.ExecuteReaderAsync();
                    while (await peerReader.ReadAsync())
                    {
                        blocks.Add(await GetBlock(peerReader, groupedTransactions));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read block from local database", e);
            }
            return blocks;
        }

        public async Task AddBlock(IBlock block)
        {
            try
            {
                using (var transaction = Connection.BeginTransaction())
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO block " +
                        "(id, version, timestamp, previous_block_id, total_amount, total_fee, payload_length, generator_public_key, " +
                        "previous_block_hash, cumulative_difficulty, base_target, next_block_id, height, generation_signature, " +
                        "block_signature, payload_hash, generator_id) " +
                        "VALUES " +
                        "(@id, @version, @timestamp, @previous_block_id, @total_amount, @total_fee, " +
                        "@payload_length, @generator_public_key, @previous_block_hash, @cumulative_difficulty, @base_target, " +
                        "@next_block_id, @height, @generation_signature, @block_signature, @payload_hash, @generator_id) ";

                    cmd.Parameters.Add("@id", DbType.Int64).Value = block.Id;
                    cmd.Parameters.Add("@version", DbType.Int32).Value = block.Version;
                    cmd.Parameters.Add("@timestamp", DbType.Int32).Value = block.Timestamp;
                    cmd.Parameters.Add("@previous_block_id", DbType.Int64).Value = block.PreviousBlockId;
                    cmd.Parameters.Add("@total_amount", DbType.Int64).Value = block.TotalAmount;
                    cmd.Parameters.Add("@total_fee", DbType.Int64).Value = block.TotalFee;
                    cmd.Parameters.Add("@payload_length", DbType.Int32).Value = block.PayloadLength;
                    cmd.Parameters.Add("@generator_public_key", DbType.Binary).Value = block.GeneratorPublicKey;
                    cmd.Parameters.Add("@previous_block_hash", DbType.Binary).Value = block.PreviousBlockHash;
                    cmd.Parameters.Add("@cumulative_difficulty", DbType.Binary).Value = block.CumulativeDifficulty.ToByteArray();
                    cmd.Parameters.Add("@base_target", DbType.Int64).Value = block.BaseTarget;
                    cmd.Parameters.Add("@next_block_id", DbType.Int64).Value = block.NextBlockId;
                    cmd.Parameters.Add("@height", DbType.Int32).Value = block.Height;
                    cmd.Parameters.Add("@generation_signature", DbType.Binary).Value = block.GenerationSignature;
                    cmd.Parameters.Add("@block_signature", DbType.Binary).Value = block.BlockSignature;
                    cmd.Parameters.Add("@payload_hash", DbType.Binary).Value = block.PayloadHash;
                    cmd.Parameters.Add("@generator_id", DbType.Int64).Value = block.GeneratorId;

                    await cmd.ExecuteNonQueryAsync();
                    await _transactionRepository.AddTransactions(block.Transactions);
                    await UpdatePreviousBlock(block);

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to save block to local database", e);
            }
        }

        private async Task UpdatePreviousBlock(IBlock block)
        {
            if (block.PreviousBlockId == null)
                return;

            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "UPDATE block SET next_block_id = @next_block_id WHERE id = @id";

                    cmd.Parameters.Add("@id", DbType.Int64).Value = block.PreviousBlockId;
                    cmd.Parameters.Add("@next_block_id", DbType.Int64).Value = block.Id;

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to save block to local database", e);
            }
        }

        private async Task<IBlock> GetBlock(IDataRecord peerReader, IDictionary<long, List<ITransaction>> transactionsDictionary = null)
        {
            var rowId = (long)peerReader["rowid"];
            var blockId = (long)peerReader["id"];
            var version = (int)(long)peerReader["version"];
            var timestamp = (int)(long)peerReader["timestamp"];
            var previousBlockId = (peerReader["previous_block_id"] is DBNull) ? null : (long?)peerReader["previous_block_id"];
            var totalAmount = (long)peerReader["total_amount"];
            var totalFee = (long)peerReader["total_fee"];
            var payloadLength = (int)(long)peerReader["payload_length"];
            var generatorPublicKey = (byte[])peerReader["generator_public_key"];
            var previousBlockHash = (peerReader["previous_block_hash"] is DBNull)
                ? new byte[0]
                : (byte[])peerReader["previous_block_hash"];
            var cumulativeDifficulty = new BigInteger((byte[])peerReader["cumulative_difficulty"]);
            var baseTarget = (long)peerReader["base_target"];
            var nextBlockId = (peerReader["next_block_id"] is DBNull) ? null : (long?)peerReader["next_block_id"];
            var height = (int)(long)peerReader["height"];
            var generationSignature = (byte[])peerReader["generation_signature"];
            var blockSignature = (byte[])peerReader["block_signature"];
            var payloadHash = (byte[])peerReader["payload_hash"];
            var generatorId = (long)peerReader["generator_id"];

            ReadOnlyCollection<ITransaction> transactions;
            if (transactionsDictionary == null)
                transactions = new ReadOnlyCollection<ITransaction>(await _transactionRepository.GetTransactions(blockId));
            else
            {
                List<ITransaction> txList;
                transactions = transactionsDictionary.TryGetValue(blockId, out txList)
                    ? new ReadOnlyCollection<ITransaction>(txList)
                    : new ReadOnlyCollection<ITransaction>(new ITransaction[0]);
            }


            var block = _blockFactory.Create(version, timestamp, previousBlockId, totalAmount, totalFee, payloadLength,
                payloadHash, generatorPublicKey,
                generationSignature, blockSignature, previousBlockHash, transactions, cumulativeDifficulty,
                baseTarget, nextBlockId, height, blockId, generatorId, rowId);

            return block;
        }
    }
}
