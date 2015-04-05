using System.IO;

namespace Nxt.NET.Transaction
{
    public interface IArbitraryMessageAttachment : IAttachment
    {
        byte[] MessageBytes { get; }
    }

    public class ArbitraryMessageAttachment : IArbitraryMessageAttachment
    {
        public byte[] MessageBytes { get; private set; }
        public int Length { get; private set; }

        public ArbitraryMessageAttachment(byte[] messageBytes)
        {
            MessageBytes = messageBytes;
            Length = MessageBytes.Length + 4;
        }

        public byte[] GetBytes()
        {
            using (var memoryStream = new MemoryStream(Length))
            {
                memoryStream.Write(MessageBytes.Length);
                memoryStream.Write(MessageBytes);
                return memoryStream.ToArray();
            }
        }
    }
}