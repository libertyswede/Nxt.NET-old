using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Nxt.NET.Transaction;

namespace Nxt.NET
{
    public interface ITransactionRepository
    {
        Task<ITransaction> GetTransaction(long id);
        Task<ITransaction> GetTransactionByFullHash(string hash);
        Task<bool> HasTransaction(long id);
        Task<IList<ITransaction>> GetTransactions(long blockid);
        Task<IList<ITransaction>> GetAllTransactions();
        Task AddTransaction(ITransaction transaction);
        Task AddTransactions(IReadOnlyCollection<ITransaction> transactions);
    }

    public class TransactionRepository : RepositoryBase, ITransactionRepository
    {
        private readonly ITransactionFactory _transactionFactory;

        public TransactionRepository(IDbController dbController, ITransactionFactory transactionFactory)
            : base(dbController)
        {
            _transactionFactory = transactionFactory;
        }

        public async Task<ITransaction> GetTransaction(long id)
        {
            ITransaction transaction = null;

            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT rowid, id, deadline, sender_public_key, recipient_id, amount, fee, height, " +
                        "block_id, signature, timestamp, block_timestamp, type, subtype, sender_id, " +
                        "full_hash, referenced_transaction_full_hash, attachment_bytes " +
                        "FROM [transaction] WHERE id = @id";

                    cmd.Parameters.Add(new SQLiteParameter("@id") { Value = id });
                    var peerReader = await cmd.ExecuteReaderAsync();
                    if (await peerReader.ReadAsync())
                    {
                        transaction = GetTransaction(peerReader);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read transaction from local database", e);
            }
            return transaction;
        }

        public async Task<ITransaction> GetTransactionByFullHash(string hash)
        {
            ITransaction transaction = null;

            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT rowid, id, deadline, sender_public_key, recipient_id, amount, fee, height, " +
                        "block_id, signature, timestamp, block_timestamp, type, subtype, sender_id, " +
                        "full_hash, referenced_transaction_full_hash, attachment_bytes " +
                        "FROM [transaction] WHERE full_hash = @hash";

                    cmd.Parameters.Add(new SQLiteParameter("@full_hash") { Value = hash });
                    var peerReader = await cmd.ExecuteReaderAsync();
                    if (await peerReader.ReadAsync())
                    {
                        transaction = GetTransaction(peerReader);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read transaction from local database", e);
            }
            return transaction;
        }

        public async Task<bool> HasTransaction(long id)
        {
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1 FROM [transaction] WHERE id = @id";
                    cmd.Parameters.Add(new SQLiteParameter("@id") { Value = id });
                    var peerReader = await cmd.ExecuteScalarAsync();
                    return peerReader != null;
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read transaction from local database", e);
                return false;
            }
        }

        public async Task<IList<ITransaction>> GetTransactions(long blockid)
        {
            IList<ITransaction> transactions = new List<ITransaction>();
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT rowid, id, deadline, sender_public_key, recipient_id, amount, fee, height, " +
                        "block_id, signature, timestamp, block_timestamp, type, subtype, sender_id, " +
                        "full_hash, referenced_transaction_full_hash, attachment_bytes " +
                        "FROM [transaction] WHERE block_id = @blockid";

                    cmd.Parameters.Add(new SQLiteParameter("@blockid") { Value = blockid });
                    var peerReader = await cmd.ExecuteReaderAsync();
                    while (await peerReader.ReadAsync())
                    {
                        transactions.Add(GetTransaction(peerReader));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read block from local database", e);
            }
            return transactions;
        }

        public async Task<IList<ITransaction>> GetAllTransactions()
        {
            IList<ITransaction> transactions = new List<ITransaction>();
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT rowid, id, deadline, sender_public_key, recipient_id, amount, fee, height, " +
                        "block_id, signature, timestamp, block_timestamp, type, subtype, sender_id, " +
                        "full_hash, referenced_transaction_full_hash, attachment_bytes " +
                        "FROM [transaction] ORDER BY id ASC, timestamp ASC";

                    var peerReader = await cmd.ExecuteReaderAsync();
                    while (await peerReader.ReadAsync())
                    {
                        transactions.Add(GetTransaction(peerReader));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to read block from local database", e);
            }
            return transactions;
        }

        public async Task AddTransaction(ITransaction transaction)
        {
            try
            {
                using (var cmd = Connection.CreateCommand())
                {
                    await AddTransaction(transaction, cmd);
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to save transaction to local database", e);
            }
        }

        public async Task AddTransactions(IReadOnlyCollection<ITransaction> transactions)
        {
            try
            {
                foreach (var transaction in transactions)
                {
                    using (var cmd = Connection.CreateCommand())
                    {
                        await AddTransaction(transaction, cmd);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.FatalException("Unable to save transaction to local database", e);
            }
        }

        private ITransaction GetTransaction(IDataRecord peerReader)
        {
            var rowId = (long)peerReader["rowid"];
            var txId = (long)peerReader["id"];
            var deadline = (short)(long)peerReader["deadline"];
            var senderPublicKey = (byte[])peerReader["sender_public_key"];
            var recipientId = (long)peerReader["recipient_id"];
            var amount = (long)peerReader["amount"];
            var fee = (long)peerReader["fee"];
            var height = (int)(long)peerReader["height"];
            var blockId = (long)peerReader["block_id"];
            var signature = (byte[])peerReader["signature"];
            var timestamp = (int)(long)peerReader["timestamp"];
            var blockTimestamp = (int)(long)peerReader["block_timestamp"];
            var type = (byte)(long)peerReader["type"];
            var subType = (byte)(long)peerReader["subtype"];
            var senderId = (long)peerReader["sender_id"];
            var fullHash = (byte[])peerReader["full_hash"];
            var referencedTransactionFullHash = (peerReader["referenced_transaction_full_hash"] is DBNull) ? null : (byte[])peerReader["referenced_transaction_full_hash"];
            var attachmentBytes = (peerReader["attachment_bytes"] is DBNull) ? null : (byte[])peerReader["attachment_bytes"];

            var transaction = _transactionFactory.Create(type, subType, timestamp,
                deadline, senderPublicKey, recipientId, amount, fee, referencedTransactionFullHash,
                signature, blockId, height, txId, senderId, blockTimestamp, fullHash, attachmentBytes);

            transaction.RowId = rowId;

            return transaction;
        }

        private static async Task AddTransaction(ITransaction transaction, SQLiteCommand cmd)
        {
            cmd.CommandText = "INSERT INTO [transaction] " +
                                      "(id, deadline, sender_public_key, recipient_id, amount, fee, height, " +
                                      "block_id, signature, timestamp, block_timestamp, type, subtype, sender_id, " +
                                      "full_hash, referenced_transaction_full_hash, attachment_bytes)" +
                                      "VALUES " +
                                      "(@id, @deadline, @sender_public_key, @recipient_id, @amount, @fee, @height, " +
                                      "@block_id, @signature, @timestamp, @block_timestamp, @type, @subtype, @sender_id, " +
                                      "@full_hash, @referenced_transaction_full_hash, @attachment_bytes)";

            cmd.Parameters.Add("@id", DbType.Int64).Value = transaction.Id;
            cmd.Parameters.Add("@deadline", DbType.Int16).Value = transaction.Deadline;
            cmd.Parameters.Add("@sender_public_key", DbType.Binary).Value = transaction.SenderPublicKey;
            cmd.Parameters.Add("@recipient_id", DbType.Int64).Value = transaction.RecipientId;
            cmd.Parameters.Add("@amount", DbType.Int64).Value = transaction.AmountNQT;
            cmd.Parameters.Add("@fee", DbType.Int64).Value = transaction.FeeNQT;
            cmd.Parameters.Add("@height", DbType.Int32).Value = transaction.Height;
            cmd.Parameters.Add("@block_id", DbType.Int64).Value = transaction.BlockId;
            cmd.Parameters.Add("@signature", DbType.Binary).Value = transaction.Signature;
            cmd.Parameters.Add("@timestamp", DbType.Int32).Value = transaction.Timestamp;
            cmd.Parameters.Add("@block_timestamp", DbType.Int32).Value = transaction.BlockTimestamp;
            cmd.Parameters.Add("@type", DbType.Byte).Value = transaction.TransactionType.GetTypeByte();
            cmd.Parameters.Add("@subtype", DbType.Byte).Value = transaction.TransactionType.GetSubtypeByte();
            cmd.Parameters.Add("@sender_id", DbType.Int64).Value = transaction.SenderId;
            cmd.Parameters.Add("@full_hash", DbType.Binary).Value = transaction.FullHash;
            cmd.Parameters.Add("@referenced_transaction_full_hash", DbType.Binary).Value = transaction.ReferencedTransactionFullHash;
            cmd.Parameters.Add("@attachment_bytes", DbType.Binary).Value = transaction.Attachment == null ? null : transaction.Attachment.GetBytes();

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
