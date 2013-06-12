namespace System.Threading
{
    using System;

    internal sealed class OverlappedDataCacheLine
    {
        internal const short CacheSize = 0x10;
        internal OverlappedData[] m_items = new OverlappedData[0x10];
        internal OverlappedDataCacheLine m_next;
        private bool m_removed;

        internal OverlappedDataCacheLine()
        {
            new object();
            for (short i = 0; i < 0x10; i = (short) (i + 1))
            {
                this.m_items[i] = new OverlappedData(this);
                this.m_items[i].m_slot = i;
            }
            new object();
        }

        ~OverlappedDataCacheLine()
        {
            this.m_removed = true;
        }

        internal bool Removed
        {
            get
            {
                return this.m_removed;
            }
            set
            {
                this.m_removed = value;
            }
        }
    }
}

