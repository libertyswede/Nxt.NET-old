using System.IO;

namespace Nxt.NET.Transaction
{
    public interface IEffectiveBalanceLeasingAttachment : IAttachment
    {
        short Period { get; }
    }

    public class EffectiveBalanceLeasingAttachment : IEffectiveBalanceLeasingAttachment
    {
        public short Period { get; private set; }
        public int Length { get { return 2; } }

        public EffectiveBalanceLeasingAttachment(short period)
        {
            Period = period;
        }

        public byte[] GetBytes()
        {
            using (var memoryStream = new MemoryStream(2))
            {
                memoryStream.Write(Period);
                return memoryStream.ToArray();
            }
        }
    }
}
