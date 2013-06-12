namespace System.Threading
{
    using System;

    internal class ReaderWriterCount
    {
        public ReaderWriterCount next;
        public RecursiveCounts rc;
        public int readercount;
        public int threadid = -1;

        public ReaderWriterCount(bool fIsReentrant)
        {
            if (fIsReentrant)
            {
                this.rc = new RecursiveCounts();
            }
        }
    }
}

