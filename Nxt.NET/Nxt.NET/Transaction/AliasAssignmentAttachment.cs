using System.IO;
using System.Text;

namespace Nxt.NET.Transaction
{
    public interface IAliasAssignmentAttachment : IAttachment
    {
        string AliasName { get; }
        string AliasUri { get; }
    }

    public class AliasAssignmentAttachment : IAliasAssignmentAttachment
    {
        public string AliasName { get; private set; }
        public string AliasUri { get; private set; }
        public int Length { get; private set; }

        public AliasAssignmentAttachment(string aliasName, string aliasUri)
        {
            AliasName = aliasName;
            AliasUri = aliasUri;
            Length = CalculateLength(aliasName, aliasUri);
        }

        private static int CalculateLength(string aliasName, string aliasUri)
        {
            return 1 + Encoding.UTF8.GetBytes(aliasName).Length + 2 + Encoding.UTF8.GetBytes(aliasUri).Length;
        }

        public byte[] GetBytes()
        {
            using (var memoryStream = new MemoryStream(Length))
            {
                var nameBytes = Encoding.UTF8.GetBytes(AliasName);
                var uriBytes = Encoding.UTF8.GetBytes(AliasUri);

                memoryStream.Write((sbyte)nameBytes.Length);
                memoryStream.Write(nameBytes);
                memoryStream.Write((short)uriBytes.Length);
                memoryStream.Write(uriBytes);

                return memoryStream.ToArray();
            }
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Uri: {1}", AliasName, AliasUri);
        }
    }
}