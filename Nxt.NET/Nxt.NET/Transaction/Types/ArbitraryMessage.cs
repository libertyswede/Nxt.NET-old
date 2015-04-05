using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nxt.NET.Transaction.Types
{
    public class ArbitraryMessage : Messaging
    {
        public ArbitraryMessage(IConfiguration configuration) : base(configuration)
        {
        }

        public override byte GetSubtypeByte()
        {
            return SubtypeMessagingArbitraryMessage;
        }

        public override void LoadAttachment(ITransaction transaction, JToken attachmentData)
        {
            var message = (string) attachmentData.SelectToken("message");
            transaction.Attachment = new ArbitraryMessageAttachment(message.ToByteArray());
        }

        public override void LoadAttachment(ITransaction transaction, byte[] attachmentData)
        {
            var messageLength = BitConverter.ToInt32(attachmentData, 0);
            if (messageLength > Constants.MaxArbitraryMessageLength)
            {
                throw new NxtValidationException("Invalid arbitrary message length: " + messageLength);
            }
            var message = attachmentData.Skip(4).Take(messageLength).ToArray();
            transaction.Attachment = new ArbitraryMessageAttachment(message);
        }

        public override void ValidateAttachment(ITransaction transaction)
        {
            var attachment = (IArbitraryMessageAttachment) transaction.Attachment;
            if (transaction.AmountNQT != 0 || attachment.MessageBytes.Length > Constants.MaxArbitraryMessageLength)
                throw new NxtValidationException("Invalid arbitrary message for transaction: " + transaction.PresentationId);
        }

        protected override void ApplyAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
        }

        protected override void UndoAttachment(ITransaction transaction, IAccount senderAccount, IAccount recipientAccount)
        {
        }
    }
}
