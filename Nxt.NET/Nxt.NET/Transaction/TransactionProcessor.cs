using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nxt.NET.Block;

namespace Nxt.NET.Transaction
{
    public interface ITransactionProcessor
    {
        IList<ITransaction> GetGenesisTransactions();
        void ApplyTransactions(IBlock block);
        Task VerifyTransactions(IBlock block);
    }

    public class TransactionProcessor : ITransactionProcessor
    {
        private readonly ITransactionFactory _transactionFactory;
        private readonly IGenesis _genesis;
        private readonly ITransactionVerifyer _transactionVerifyer;

        public TransactionProcessor(ITransactionFactory transactionFactory, IGenesis genesis, ITransactionVerifyer transactionVerifyer)
        {
            _transactionFactory = transactionFactory;
            _genesis = genesis;
            _transactionVerifyer = transactionVerifyer;
        }

        public IList<ITransaction> GetGenesisTransactions()
        {
            var transactions = _genesis.GetGenesisTransactions(_transactionFactory);
            return transactions;
        }

        public void ApplyTransactions(IBlock block)
        {
            block.Transactions.ToList().ForEach(t => t.Apply());
        }

        public async Task VerifyTransactions(IBlock block)
        {
            await _transactionVerifyer.VerifyTransactions(block);
        }
    }
}
