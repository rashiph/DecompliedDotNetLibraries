namespace System.Transactions
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct VolatileEnlistmentSet
    {
        internal InternalEnlistment[] volatileEnlistments;
        internal int volatileEnlistmentCount;
        internal int volatileEnlistmentSize;
        internal int dependentClones;
        internal int preparedVolatileEnlistments;
        private VolatileDemultiplexer volatileDemux;
        internal VolatileDemultiplexer VolatileDemux
        {
            get
            {
                return this.volatileDemux;
            }
            set
            {
                this.volatileDemux = value;
            }
        }
    }
}

