using System.Linq;
using System.Numerics;
using Newtonsoft.Json.Linq;
using NLog;
using Nxt.NET.Crypto;
using StructureMap;

namespace Nxt.NET.Transaction
{
    public interface ITransactionFactory
    {
        ITransaction Create(byte type, byte subType, int timestamp, short deadline, byte[] senderPublicKey,
            long recipientId, long amountNQT, long feeNQT, byte[] referencedTransactionFullHash, byte[] signature);

        ITransaction Create(byte type, byte subType, int timestamp, short deadline, byte[] senderPublicKey,
            long recipientId, long amountNQT, long feeNQT, byte[] referencedTransactionFullHash, byte[] signature,
            JToken attachmentData);

        ITransaction Create(byte type, byte subType, int timestamp, short deadline, byte[] senderPublicKey,
            long recipientId, long amountNQT, long feeNQT, byte[] referencedTransactionFullHash, byte[] signature,
            long blockId, int height, long id, long senderId, int blockTimestamp, byte[] fullHash, byte[] attachmentData);
    }

    public class TransactionFactory : ITransactionFactory
    {
        private readonly ICryptoFactory _cryptoFactory;
        private readonly ITransactionTypeFactory _transactionTypeFactory;
        private readonly IConvert _convert;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly JToken EmptyAttachment = JToken.Parse("{}");

        public TransactionFactory(ICryptoFactory cryptoFactory, ITransactionTypeFactory transactionTypeFactory,
            IConvert convert)
        {
            _cryptoFactory = cryptoFactory;
            _transactionTypeFactory = transactionTypeFactory;
            _convert = convert;
        }

        public ITransaction Create(byte type, byte subType, int timestamp, short deadline, byte[] senderPublicKey, long recipientId,
            long amountNQT, long feeNQT, byte[] referencedTransactionFullHash, byte[] signature)
        {
            return Create(type, subType, timestamp, deadline, senderPublicKey, recipientId, amountNQT, feeNQT,
                referencedTransactionFullHash, signature, EmptyAttachment);
        }

        public ITransaction Create(byte type, byte subType, int timestamp, short deadline, byte[] senderPublicKey,
            long recipientId, long amountNQT, long feeNQT, byte[] referencedTransactionFullHash, byte[] signature,
            JToken attachmentData)
        {
            var transactionType = _transactionTypeFactory.FindTransactionType(type, subType);

            var transaction = ObjectFactory.GetInstance<ITransaction>();
            transaction.TransactionType = transactionType;
            transaction.Timestamp = timestamp;
            transaction.Deadline = deadline;
            transaction.SenderPublicKey = senderPublicKey;
            transaction.RecipientId = recipientId;
            transaction.AmountNQT = amountNQT;
            transaction.FeeNQT = feeNQT;
            transaction.ReferencedTransactionFullHash = referencedTransactionFullHash;
            transaction.Signature = signature;
            transaction.SenderId = GenerateSenderId(senderPublicKey);

            transactionType.LoadAttachment(transaction, attachmentData);
            GenerateId(transaction);
            ValidateTransaction(transaction);

            return transaction;
        }

        public ITransaction Create(byte type, byte subType, int timestamp, short deadline, byte[] senderPublicKey,
            long recipientId, long amountNQT, long feeNQT, byte[] referencedTransactionFullHash, byte[] signature,
            long blockId, int height, long id, long senderId, int blockTimestamp, byte[] fullHash, byte[] attachmentData)
        {
            var transactionType = _transactionTypeFactory.FindTransactionType(type, subType);

            var transaction = ObjectFactory.GetInstance<ITransaction>();
            transaction.TransactionType = transactionType;
            transaction.Timestamp = timestamp;
            transaction.Deadline = deadline;
            transaction.SenderPublicKey = senderPublicKey;
            transaction.RecipientId = recipientId;
            transaction.AmountNQT = amountNQT;
            transaction.FeeNQT = feeNQT;
            transaction.ReferencedTransactionFullHash = referencedTransactionFullHash;
            transaction.Signature = signature;
            transaction.BlockId = blockId;
            transaction.Height = height;
            transaction.Id = id;
            transaction.SenderId = senderId;
            transaction.BlockTimestamp = blockTimestamp;
            transaction.FullHash = fullHash;

            if (attachmentData != null)
                transactionType.LoadAttachment(transaction, attachmentData);

            GenerateId(transaction);
            ValidateTransaction(transaction);

            return transaction;
        }

        public void GenerateId(ITransaction transaction)
        {
            var crypto = _cryptoFactory.Create();
            if (transaction.Signature == null || transaction.Signature.Length == 0)
            {
                throw new NxtException("Transaction is not signed yet");
            }

            var bytes = transaction.GetBytes();
            transaction.FullHash = crypto.ComputeHash(bytes);
            var idHash = new[]
            {
                transaction.FullHash[0], transaction.FullHash[1], transaction.FullHash[2], transaction.FullHash[3],
                transaction.FullHash[4], transaction.FullHash[5], transaction.FullHash[6], transaction.FullHash[7]
            };
            var big = new BigInteger(idHash);
            transaction.Id = (long)big;

            Logger.Debug(string.Format("Transaction Id:{0}, Bytes:{1}, Hash bytes:{2}", transaction.Id,
                bytes.Aggregate(string.Empty, (current, b) => current + (b + ", ")),
                idHash.Aggregate(string.Empty, (current, b) => current + (b + ", "))));
        }

        private long GenerateSenderId(byte[] publicKey)
        {
            var crypto = _cryptoFactory.Create();
            var publicKeyHash = crypto.ComputeHash(publicKey);
            return _convert.FullHashToId(publicKeyHash);
        }

        // TODO: Make this readable (ie, refactor!)
        private static void ValidateTransaction(ITransaction transaction)
        {
            var isGenesisTransaction = transaction.IsGenesisTransaction();
            if (((isGenesisTransaction && (transaction.Deadline != 0 || transaction.FeeNQT != 0))
                  || (!isGenesisTransaction && (transaction.Deadline < 1 || transaction.FeeNQT < Constants.OneNxt)))
                  || transaction.FeeNQT > Constants.MaxBalanceNqt
                  || transaction.AmountNQT < 0
                  || transaction.AmountNQT > Constants.MaxBalanceNqt)
            {
                throw new NxtException(
                    string.Format(
                        "Invalid transaction parameters:\n type: {0}, timestamp: {1}, deadline: {2}, fee: {3}, amount: {4}",
                        transaction.TransactionType, transaction.Timestamp, transaction.Deadline, transaction.FeeNQT,
                        transaction.AmountNQT));
            }
        }
    }
}
