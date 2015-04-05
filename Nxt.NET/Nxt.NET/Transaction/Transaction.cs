using System.Collections.Generic;
using System.Linq;
using Nxt.NET.Transaction.Types;

namespace Nxt.NET.Transaction
{
    public interface ITransaction
    {
        long RowId { get; set; }
        long Id { get; set; }
        ulong PresentationId { get; }
        ITransactionType TransactionType { get; set; }
        int Timestamp { get; set; }
        short Deadline { get; set; }
        byte[] SenderPublicKey { get; set; }
        long RecipientId { get; set; }
        long AmountNQT { get; set; }
        long FeeNQT { get; set; }
        byte[] Signature { get; set; }
        byte[] ReferencedTransactionFullHash { get; set; }
        int Height { get; set; }
        long SenderId { get; set; }
        int BlockTimestamp { get; set; }
        byte[] FullHash { get; set; }
        long BlockId { get; set; }
        IAttachment Attachment { get; set; }

        bool IsGenesisTransaction();
        int GetExpiration();
        byte[] GetBytes(bool zeroSignature = false);
        bool UseNQT();
        void ValidateAttachment();
        bool ApplyUnconfirmed();
        void UndoUnconfirmed();
        void Apply();
        bool IsDuplicate(IDictionary<ITransactionType, HashSet<string>> duplicates);
    }

    public class Transaction : ITransaction
    {
        private readonly ITransactionByteSerializer _transactionByteSerializer;
        private readonly IAccountContainer _accountContainer;
        public long RowId { get; set; }
        public long Id { get; set; }
        public ulong PresentationId { get { return (ulong)Id; } }
        public long BlockId { get; set; }
        public int Height { get; set; }
        public long SenderId { get; set; }
        public int BlockTimestamp { get; set; }
        public byte[] FullHash { get; set; }
        public ITransactionType TransactionType { get; set; }
        public int Timestamp { get; set; }
        public short Deadline { get; set; }
        public byte[] SenderPublicKey { get; set; }
        public long RecipientId { get; set; }
        public long AmountNQT { get; set; }
        public long FeeNQT { get; set; }
        public byte[] Signature { get; set; }
        public byte[] ReferencedTransactionFullHash { get; set; }
        public IAttachment Attachment { get; set; }

        public Transaction(ITransactionByteSerializer transactionByteSerializer, IAccountContainer accountContainer)
        {
            _transactionByteSerializer = transactionByteSerializer;
            _accountContainer = accountContainer;
        }

        public byte[] GetBytes(bool zeroSignature = false)
        {
            return _transactionByteSerializer.SerializeBytes(this, zeroSignature);
        }

        public bool UseNQT()
        {
            return _transactionByteSerializer.UseNQT(this);
        }

        public void ValidateAttachment()
        {
            TransactionType.ValidateAttachment(this);
        }

        public bool IsGenesisTransaction()
        {
            return (Timestamp == 0 && SenderPublicKey.SequenceEqual(Genesis.CreatorPublicKey));
        }

        public int GetExpiration()
        {
            return Timestamp + Deadline*60;
        }

        public bool ApplyUnconfirmed()
        {
            var senderAccount = _accountContainer.GetAccount(SenderId);
            return senderAccount != null && TransactionType.ApplyUnconfirmed(this, senderAccount);
        }

        public void Apply()
        {
            var senderAccount = _accountContainer.GetAccount(SenderId);
            senderAccount.ApplyPublicKey(SenderPublicKey, Height);
            var recipientAccount = _accountContainer.GetOrAddAccount(RecipientId);
            TransactionType.Apply(this, senderAccount, recipientAccount);
        }

        public bool IsDuplicate(IDictionary<ITransactionType, HashSet<string>> duplicates)
        {
            return TransactionType.IsDuplicate(this, duplicates);
        }

        public void UndoUnconfirmed()
        {
            var senderAccount = _accountContainer.GetAccount(SenderId);
            TransactionType.UndoUnconfirmed(this, senderAccount);
        }
    }
}