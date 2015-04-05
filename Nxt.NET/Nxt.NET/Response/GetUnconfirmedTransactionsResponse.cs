using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nxt.NET.Transaction;

namespace Nxt.NET.Response
{
    public interface IGetUnconfirmedTransactionsResponse
    {
        IReadOnlyCollection<ITransaction> Transactions { get; }
    }

    public class GetUnconfirmedTransactionsResponse : IGetUnconfirmedTransactionsResponse
    {
        private readonly ITransactionParser _transactionParser;
        private readonly IList<ITransaction> _transactions = new List<ITransaction>();
        public IReadOnlyCollection<ITransaction> Transactions { get; private set; }

        public GetUnconfirmedTransactionsResponse(ITransactionParser transactionParser, string json)
        {
            _transactionParser = transactionParser;
            ReadTransactions(json);
        }

        private void ReadTransactions(string json)
        {
            var transactionsToken = JObject.Parse(json).SelectToken("unconfirmedTransactions");
            var transactions = _transactionParser.ParseTransactions(transactionsToken).ToList();
            transactions.ForEach(t => _transactions.Add(t));
            Transactions = new ReadOnlyCollection<ITransaction>(_transactions);
        }
    }
}