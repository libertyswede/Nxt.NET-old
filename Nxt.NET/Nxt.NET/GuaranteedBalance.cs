using System;

namespace Nxt.NET
{
    public class GuaranteedBalance : IComparable<GuaranteedBalance>
    {
        public int Height { get; private set; }
        public long Balance { get; set; }
        public bool Ignore { get; set; }

        public GuaranteedBalance(int height, long balance)
        {
            Height = height;
            Balance = balance;
            Ignore = false;
        }

        public int CompareTo(GuaranteedBalance other)
        {
            if (Height < other.Height)
            {
                return -1;
            }
            if (Height > other.Height)
            {
                return 1;
            }
            return 0;
        }
    }
}
