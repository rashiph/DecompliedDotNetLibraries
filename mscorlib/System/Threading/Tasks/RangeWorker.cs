namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RangeWorker
    {
        internal readonly IndexRange[] m_indexRanges;
        internal int m_nCurrentIndexRange;
        internal long m_nStep;
        internal long m_nIncrementValue;
        internal readonly long m_nMaxIncrementValue;
        internal RangeWorker(IndexRange[] ranges, int nInitialRange, long nStep)
        {
            this.m_indexRanges = ranges;
            this.m_nCurrentIndexRange = nInitialRange;
            this.m_nStep = nStep;
            this.m_nIncrementValue = nStep;
            this.m_nMaxIncrementValue = 0x10L * nStep;
        }

        internal bool FindNewWork(out long nFromInclusiveLocal, out long nToExclusiveLocal)
        {
            int length = this.m_indexRanges.Length;
            do
            {
                IndexRange range = this.m_indexRanges[this.m_nCurrentIndexRange];
                if (range.m_bRangeFinished == 0)
                {
                    if (this.m_indexRanges[this.m_nCurrentIndexRange].m_nSharedCurrentIndexOffset == null)
                    {
                        Interlocked.CompareExchange<Shared<long>>(ref this.m_indexRanges[this.m_nCurrentIndexRange].m_nSharedCurrentIndexOffset, new Shared<long>(0L), null);
                    }
                    long num2 = Interlocked.Add(ref this.m_indexRanges[this.m_nCurrentIndexRange].m_nSharedCurrentIndexOffset.Value, this.m_nIncrementValue) - this.m_nIncrementValue;
                    if ((range.m_nToExclusive - range.m_nFromInclusive) > num2)
                    {
                        nFromInclusiveLocal = range.m_nFromInclusive + num2;
                        nToExclusiveLocal = nFromInclusiveLocal + this.m_nIncrementValue;
                        if ((nToExclusiveLocal > range.m_nToExclusive) || (nToExclusiveLocal < range.m_nFromInclusive))
                        {
                            nToExclusiveLocal = range.m_nToExclusive;
                        }
                        if (this.m_nIncrementValue < this.m_nMaxIncrementValue)
                        {
                            this.m_nIncrementValue *= 2L;
                            if (this.m_nIncrementValue > this.m_nMaxIncrementValue)
                            {
                                this.m_nIncrementValue = this.m_nMaxIncrementValue;
                            }
                        }
                        return true;
                    }
                    Interlocked.Exchange(ref this.m_indexRanges[this.m_nCurrentIndexRange].m_bRangeFinished, 1);
                }
                this.m_nCurrentIndexRange = (this.m_nCurrentIndexRange + 1) % this.m_indexRanges.Length;
                length--;
            }
            while (length > 0);
            nFromInclusiveLocal = 0L;
            nToExclusiveLocal = 0L;
            return false;
        }

        internal bool FindNewWork32(out int nFromInclusiveLocal32, out int nToExclusiveLocal32)
        {
            long num;
            long num2;
            bool flag = this.FindNewWork(out num, out num2);
            nFromInclusiveLocal32 = (int) num;
            nToExclusiveLocal32 = (int) num2;
            return flag;
        }
    }
}

