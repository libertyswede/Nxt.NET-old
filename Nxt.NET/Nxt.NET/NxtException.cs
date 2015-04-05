using System;
using Nxt.NET.Transaction;

namespace Nxt.NET
{
    public class NxtException : Exception
    {
        public NxtException(string message) : base(message)
        {
        }
    }

    public class BlockNotAcceptedException : NxtException
    {
        public BlockNotAcceptedException(string message) : base(message)
        {
        }
    }

    public class BlockOutOfOrderException : BlockNotAcceptedException
    {
        public BlockOutOfOrderException(string message) : base(message)
        {
        }
    }

    public class TransactionNotAcceptedException : BlockNotAcceptedException
    {
        public readonly ITransaction Transaction;
        public TransactionNotAcceptedException(string message, ITransaction transaction)
            : base(message)
        {
            Transaction = transaction;
        }
    }

    public class NxtValidationException : NxtException
    {
        public NxtValidationException(string message)
            : base(message)
        {
        }
    }

    public class UndoNotSupportedException : NxtException
    {
        public UndoNotSupportedException(string message)
            : base(message)
        {
        }
    }
}