using System.Threading.Tasks;
using Nxt.NET.Crypto;

namespace Nxt.NET.Transaction
{
    public interface ITransactionsChecksumCalculator
    {
        Task<byte[]> CalculateAllTransactionsChecksum();
    }

    public class TransactionsChecksumCalculator : ITransactionsChecksumCalculator
    {
        private readonly ICryptoFactory _cryptoFactory;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionsChecksumCalculator(ICryptoFactory cryptoFactory, ITransactionRepository transactionRepository)
        {
            _cryptoFactory = cryptoFactory;
            _transactionRepository = transactionRepository;
        }

        public async Task<byte[]> CalculateAllTransactionsChecksum()
        {
            var crypto = _cryptoFactory.Create();
            var transactions = await _transactionRepository.GetAllTransactions();
            for (var i = 0; i < transactions.Count - 1; i++)
            {
                crypto.TransformBlock(transactions[i].GetBytes());
            }
            crypto.TransformFinalBlock(transactions[transactions.Count - 1].GetBytes());
            return crypto.Hash;
        }
    }
}
