using System.IO;

namespace Nxt.NET.Block
{
    public interface IBlockByteSerializer
    {
        byte[] SerializeBytes(IBlock block);
    }

    public class BlockByteSerializer : IBlockByteSerializer
    {
        public byte[] SerializeBytes(IBlock block)
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(block.Version);
                memoryStream.Write(block.Timestamp);
                memoryStream.Write(GetNullToZeroPreviousBlockId(block));
                memoryStream.Write(block.Transactions.Count);
                if (block.Version < 3)
                {
                    memoryStream.Write((int)(block.TotalAmount / Constants.OneNxt));
                    memoryStream.Write((int)(block.TotalFee / Constants.OneNxt));
                }
                else
                {
                    memoryStream.Write(block.TotalAmount);
                    memoryStream.Write(block.TotalFee);
                }
                memoryStream.Write(block.PayloadLength);
                memoryStream.Write(block.PayloadHash);
                memoryStream.Write(block.GeneratorPublicKey);
                memoryStream.Write(block.GenerationSignature);
                if (block.Version > 1)
                {
                    memoryStream.Write(block.PreviousBlockHash);
                }
                memoryStream.Write(block.BlockSignature);

                return memoryStream.ToArray();
            }
        }

        private long GetNullToZeroPreviousBlockId(IBlock block)
        {
            return block.PreviousBlockId == null ? 0L : (long)block.PreviousBlockId;
        }
    }
}
