using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Transaction
{
    public interface ITransactionParser
    {
        IList<ITransaction> ParseTransactions(JToken jsonToken);
    }

    public class TransactionParser : ITransactionParser
    {
        private readonly ITransactionFactory _transactionFactory;

        public TransactionParser(ITransactionFactory transactionFactory)
        {
            _transactionFactory = transactionFactory;
        }

        public IList<ITransaction> ParseTransactions(JToken jsonToken)
        {
            var transactions = new List<ITransaction>();
            var currentTransactionToken = jsonToken.FirstOrDefault();

            while (currentTransactionToken != null)
            {
                var type = (byte)currentTransactionToken.SelectToken("type", false);
                var subtype = (byte)currentTransactionToken.SelectToken("subtype", false);
                var timestamp = (int)currentTransactionToken.SelectToken("timestamp", false);
                var deadline = (short)currentTransactionToken.SelectToken("deadline", false);
                var senderPublicKey =
                    ((string)currentTransactionToken.SelectToken("senderPublicKey", false)).ToByteArray();
                var recipient = (long)(ulong)currentTransactionToken.SelectToken("recipient", false);
                var amountNQT = (long)currentTransactionToken.SelectToken("amountNQT", false);
                var feeNQT = (long)currentTransactionToken.SelectToken("feeNQT", false);
                var signature = ((string)currentTransactionToken.SelectToken("signature", false)).ToByteArray();
                var referencedTransactionFullHash =
                    ((string)currentTransactionToken.SelectToken("referencedTransactionFullHash", false)).ToByteArray();
                var attachment = currentTransactionToken.SelectToken("attachment", false);
                referencedTransactionFullHash = (referencedTransactionFullHash != null && referencedTransactionFullHash.Length == 0)
                                                ? null : referencedTransactionFullHash;

                var transaction = _transactionFactory.Create(type, subtype, timestamp, deadline, senderPublicKey,
                    recipient, amountNQT, feeNQT, referencedTransactionFullHash, signature, attachment);

                transactions.Add(transaction);
                currentTransactionToken = currentTransactionToken.Next;
            }

            return transactions;
        }
    }
}
