using System;

namespace Nxt.NET.Block
{
    public class ApplyBlockEventArgs : EventArgs
    {
        public IBlock Block { get; private set; }
        public ApplyBlockEventArgs(IBlock block)
        {
            Block = block;
        }
    }
}