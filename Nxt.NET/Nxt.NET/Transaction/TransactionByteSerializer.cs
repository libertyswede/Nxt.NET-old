using System.IO;
using Nxt.NET.Block;

namespace Nxt.NET.Transaction
{
    public interface ITransactionByteSerializer
    {
        byte[] SerializeBytes(ITransaction transaction, bool zeroSignature = false);
        bool UseNQT(ITransaction transaction);
    }

    public class TransactionByteSerializer : ITransactionByteSerializer
    {
        private readonly IConvert _convert;
        private readonly IBlockRepository _blockRepository;

        public TransactionByteSerializer(IConvert convert, IBlockRepository blockRepository)
        {
            _convert = convert;
            _blockRepository = blockRepository;
        }

        public byte[] SerializeBytes(ITransaction transaction, bool zeroSignature = false)
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(transaction.TransactionType.GetTypeByte());
                memoryStream.Write(transaction.TransactionType.GetSubtypeByte());
                memoryStream.Write(transaction.Timestamp);
                memoryStream.Write(transaction.Deadline);
                memoryStream.Write(transaction.SenderPublicKey);
                memoryStream.Write(transaction.RecipientId);

                if (UseNQT(transaction))
                {
                    memoryStream.Write(transaction.AmountNQT);
                    memoryStream.Write(transaction.FeeNQT);
                    memoryStream.Write(transaction.ReferencedTransactionFullHash ?? new byte[32]);
                }
                else
                {
                    memoryStream.Write((int)(transaction.AmountNQT / Constants.OneNxt));
                    memoryStream.Write((int)(transaction.FeeNQT / Constants.OneNxt));
                    memoryStream.Write(transaction.ReferencedTransactionFullHash != null ? _convert.FullHashToId(transaction.ReferencedTransactionFullHash) : 0L);
                }

                memoryStream.Write((transaction.Signature == null || zeroSignature) ? new byte[64] : transaction.Signature);
                if (transaction.Attachment != null)
                    memoryStream.Write(transaction.Attachment.GetBytes());

                return memoryStream.ToArray();
            }
        }

        public bool UseNQT(ITransaction transaction)
        {
            return transaction.Height > Constants.NQTBlock
                   && _blockRepository.LastBlock.Height >= Constants.NQTBlock;
        }
    }
}
