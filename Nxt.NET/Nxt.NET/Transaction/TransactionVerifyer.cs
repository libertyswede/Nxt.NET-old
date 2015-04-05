using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NET.Transaction
{
    public interface ITransactionVerifyer
    {
        Task VerifyTransactions(IBlock block);
        void VerifySignature(ITransaction transaction);
    }

    public class TransactionVerifyer : ITransactionVerifyer
    {
        private readonly IConvert _convert;
        private readonly IBlockRepository _blockRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IConfiguration _configuration;
        private readonly ITransactionSignatureVerifyer _transactionSignatureVerifyer;
        private readonly ICryptoFactory _cryptoFactory;

        public TransactionVerifyer(IConvert convert, IBlockRepository blockRepository,
            ITransactionRepository transactionRepository, IConfiguration configuration,
            ITransactionSignatureVerifyer transactionSignatureVerifyer, ICryptoFactory cryptoFactory)
        {
            _convert = convert;
            _blockRepository = blockRepository;
            _transactionRepository = transactionRepository;
            _configuration = configuration;
            _transactionSignatureVerifyer = transactionSignatureVerifyer;
            _cryptoFactory = cryptoFactory;
        }

        public async Task VerifyTransactions(IBlock block)
        {
            var currentTime = _convert.GetEpochTime();
            var previousLastBlock = _blockRepository.LastBlock;
            var calculatedTotalAmount = 0L;
            var calculatedTotalFee = 0L;
            var crypto = _cryptoFactory.Create();
            var lastTransaction = block.Transactions.LastOrDefault();
            var duplicates = new Dictionary<ITransactionType, HashSet<string>>();

            using (var unconfirmedTransactionApplier = ObjectFactory.GetInstance<IUnconfirmedTransactionApplier>())
            {
                foreach (var transaction in block.Transactions)
                {
                    VerifyTimestamp(block, transaction, currentTime, previousLastBlock);
                    await VerifyTransactionId(transaction);
                    await VerifyReferencedTransactions(transaction, previousLastBlock);
                    VerifySignature(transaction);
                    VerifyDuplicates(transaction, duplicates);
                    ValidateAttachment(transaction);

                    calculatedTotalAmount += transaction.AmountNQT;
                    calculatedTotalFee += transaction.FeeNQT;
                    ApplyUnconfirmedTransaction(unconfirmedTransactionApplier, transaction);

                    if (transaction != lastTransaction)
                        crypto.TransformBlock(transaction.GetBytes());
                }
                unconfirmedTransactionApplier.Commit();
            }

            crypto.TransformFinalBlock(lastTransaction != null ? lastTransaction.GetBytes() : new byte[0]);

            VerifyAmountAndFee(block, calculatedTotalAmount, calculatedTotalFee);
            VerifyPayloadHash(block, crypto);
        }

        private static void ApplyUnconfirmedTransaction(IUnconfirmedTransactionApplier unconfirmedTransactionApplier,
            ITransaction transaction)
        {
            if (!unconfirmedTransactionApplier.ApplyUnconfirmedTransaction(transaction))
            {
                throw new TransactionNotAcceptedException("Double spending transaction: " + (ulong) transaction.Id, transaction);
            }
        }

        private async Task VerifyReferencedTransactions(ITransaction transaction, IBlock previousLastBlock)
        {
            if (transaction.ReferencedTransactionFullHash != null)
            {
                if (!await VerifyPreTransactionFullHash(transaction, previousLastBlock) ||
                    !await VerifyPostTransactionFullHash(transaction, previousLastBlock))
                {
                    throw new TransactionNotAcceptedException("Missing or invalid referenced transaction "
                                                              + transaction.ReferencedTransactionFullHash
                                                              + " for transaction " + (ulong) transaction.Id, transaction);
                }
            }
        }

        private async Task<bool> VerifyPreTransactionFullHash(ITransaction transaction, IBlock previousLastBlock)
        {
            var fullHashBlock = Constants.GetReferencedTransactionFullHashBlock(_configuration);
            if (previousLastBlock.Height < fullHashBlock)
            {
                var fullHashId = _convert.FullHashToId(transaction.ReferencedTransactionFullHash);
                return await _transactionRepository.HasTransaction(fullHashId);
            }
            return true;
        }

        private async Task<bool> VerifyPostTransactionFullHash(ITransaction transaction, IBlock previousLastBlock)
        {
            var fullHashBlock = Constants.GetReferencedTransactionFullHashBlock(_configuration);
            if (previousLastBlock.Height >= fullHashBlock)
            {
                return await HasAllReferencedTransactions(transaction, transaction.Timestamp, 0);
            }
            return true;
        }

        private async Task<bool> HasAllReferencedTransactions(ITransaction transaction, int timestamp, int count)
        {
            var hash = transaction.ReferencedTransactionFullHash;
            if (hash == null)
            {
                return timestamp - transaction.Timestamp < 60 * 60 * 24 * 60 && count < 10;
            }

            transaction = await _transactionRepository.GetTransactionByFullHash(_convert.ToHexString(hash));
            return transaction != null && await HasAllReferencedTransactions(transaction, timestamp, count + 1);
        }

        private static void VerifyTimestamp(IBlock block, ITransaction transaction, int currentTime, IBlock previousLastBlock)
        {
            if (transaction.Timestamp > currentTime + 15 || transaction.Timestamp > block.Timestamp + 15 ||
                transaction.GetExpiration() < block.Timestamp && previousLastBlock.Height != 303)
            {
                throw new TransactionNotAcceptedException("Invalid transaction timestamp " + transaction.Timestamp
                                                          + " for transaction " + (ulong) transaction.Id +
                                                          ", current time is " + currentTime
                                                          + ", block timestamp is " + block.Timestamp, transaction);
            }
        }

        private async Task VerifyTransactionId(ITransaction transaction)
        {
            if (transaction.Id == 0)
            {
                throw new TransactionNotAcceptedException("Invalid transaction id", transaction);
            }
            if (await _transactionRepository.HasTransaction(transaction.Id))
            {
                throw new TransactionNotAcceptedException("Transaction " + (ulong)transaction.Id
                                                          + " is already in the blockchain", transaction);
            }
        }

        public void VerifySignature(ITransaction transaction)
        {
            if (!_transactionSignatureVerifyer.VerifyTransaction(transaction))
            {
                var previousBlockHeight = _blockRepository.LastBlock.Height;
                throw new TransactionNotAcceptedException("Signature verification failed for transaction "
                                                          + (ulong)transaction.Id + " at height " + previousBlockHeight, transaction);
            }
        }

        private static void VerifyDuplicates(ITransaction transaction, IDictionary<ITransactionType, HashSet<string>> duplicates)
        {
            if (transaction.IsDuplicate(duplicates))
            {
                throw new TransactionNotAcceptedException("Transaction is a duplicate: " + transaction.PresentationId, transaction);
            }
        }

        private static void ValidateAttachment(ITransaction transaction)
        {
            try
            {
                transaction.ValidateAttachment();
            }
            catch (NxtValidationException)
            {
                throw new TransactionNotAcceptedException("Invalid ordinary payment", transaction);
            }
        }

        private static void VerifyAmountAndFee(IBlock block, long calculatedTotalAmount, long calculatedTotalFee)
        {
            if (calculatedTotalAmount != block.TotalAmount || calculatedTotalFee != block.TotalFee)
            {
                throw new BlockNotAcceptedException("Total amount or fee don't match transaction totals");
            }
        }

        private static void VerifyPayloadHash(IBlock block, ICrypto crypto)
        {
            if (!block.PayloadHash.SequenceEqual(crypto.Hash))
            {
                throw new BlockNotAcceptedException("Payload hash doesn't match");
            }
        }
    }
}
