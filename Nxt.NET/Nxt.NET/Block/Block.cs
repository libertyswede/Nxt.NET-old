using System;
using System.Collections.Generic;
using System.Numerics;
using Nxt.NET.Transaction;

namespace Nxt.NET.Block
{
    public interface IBlock
    {
        long RowId { get; set; }
        long Id { get; set; }
        ulong PresentationId { get; }
        int Version { get; set; }
        int Timestamp { get; set; }
        long? PreviousBlockId { get; set; }
        long TotalAmount { get; set; }
        long TotalFee { get; set; }
        int PayloadLength { get; set; }
        byte[] GeneratorPublicKey { get; set; }
        byte[] BlockHash { get; set; }
        byte[] PreviousBlockHash { get; set; }
        BigInteger CumulativeDifficulty { get; set; }
        long BaseTarget { get; set; }
        long? NextBlockId { get; set; }
        int Height { get; set; }
        byte[] GenerationSignature { get; set; }
        byte[] BlockSignature { get; set; }
        byte[] PayloadHash { get; set; }
        long GeneratorId { get; set; }
        IReadOnlyCollection<ITransaction> Transactions { get; set; }
        byte[] GetBytes();
        bool IsGenesisBlock();
    }

    public class Block : IBlock
    {
        private readonly IBlockByteSerializer _blockByteSerializer;
        public long RowId { get; set; }
        public long Id { get; set; }
        public ulong PresentationId { get { return (ulong) Id; } }
        public int Version { get; set; }
        public int Timestamp { get; set; }
        public long? PreviousBlockId { get; set; }
        public long TotalAmount { get; set; }
        public long TotalFee { get; set; }
        public int PayloadLength { get; set; }
        public byte[] GeneratorPublicKey { get; set; }
        public byte[] BlockHash { get; set; }
        public byte[] PreviousBlockHash { get; set; }
        public BigInteger CumulativeDifficulty { get; set; }
        public long BaseTarget { get; set; }
        public long? NextBlockId { get; set; }
        public int Height { get; set; }
        public byte[] GenerationSignature { get; set; }
        public byte[] BlockSignature { get; set; }
        public byte[] PayloadHash { get; set; }
        public long GeneratorId { get; set; }
        public IReadOnlyCollection<ITransaction> Transactions { get; set; }

        public Block(IBlockByteSerializer blockByteSerializer)
        {
            _blockByteSerializer = blockByteSerializer;
        }

        public byte[] GetBytes()
        {
            return _blockByteSerializer.SerializeBytes(this);
        }

        public bool IsGenesisBlock()
        {
            return Id == Genesis.GenesisBlockId;
        }
    }
}
