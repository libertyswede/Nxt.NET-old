using Newtonsoft.Json.Linq;
using Nxt.NET.Transaction;

namespace Nxt.NET.Block
{
    public interface IBlockParser
    {
        IBlock ParseBlock(JToken jsonToken);
    }

    public class BlockParser: IBlockParser
    {
        private readonly IBlockFactory _blockFactory;
        private readonly ITransactionParser _transactionParser;

        public BlockParser(IBlockFactory blockFactory, ITransactionParser transactionParser)
        {
            _blockFactory = blockFactory;
            _transactionParser = transactionParser;
        }

        public IBlock ParseBlock(JToken jsonToken)
        {
            var transactions = _transactionParser.ParseTransactions(jsonToken.SelectToken("transactions"));

            var version = (int)jsonToken.SelectToken("version", false);
            var timestamp = (int)jsonToken.SelectToken("timestamp", false);
            var previousBlockId = (long)(ulong)jsonToken.SelectToken("previousBlock", false);
            var totalAmount = (long)jsonToken.SelectToken("totalAmountNQT", false);
            var totalFee = (long)jsonToken.SelectToken("totalFeeNQT", false);
            var payloadLength = (int)jsonToken.SelectToken("payloadLength", false);
            var payloadHash = ((string)jsonToken.SelectToken("payloadHash", false)).ToByteArray();
            var generatorPublicKey = ((string)jsonToken.SelectToken("generatorPublicKey", false)).ToByteArray();
            var generationSignature = ((string)jsonToken.SelectToken("generationSignature", false)).ToByteArray();
            var blockSignature = ((string)jsonToken.SelectToken("blockSignature", false)).ToByteArray();
            var previousBlockHash = ((string)jsonToken.SelectToken("previousBlockHash", false)).ToByteArray();
            previousBlockHash = (previousBlockHash != null && previousBlockHash.Length == 0) ? null : previousBlockHash;

            var block = _blockFactory.Create(version, timestamp, previousBlockId, totalAmount, totalFee, payloadLength,
                payloadHash, generatorPublicKey, generationSignature, blockSignature, previousBlockHash, transactions);

            return block;
        }
    }
}
