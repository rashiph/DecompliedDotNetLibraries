namespace System.Threading.Tasks
{
    using System;
    using System.Threading;

    internal class RangeManager
    {
        internal readonly IndexRange[] m_indexRanges;
        internal int m_nCurrentIndexRangeToAssign = 0;
        internal long m_nStep;

        internal RangeManager(long nFromInclusive, long nToExclusive, long nStep, int nNumExpectedWorkers)
        {
            this.m_nStep = nStep;
            if (nNumExpectedWorkers == 1)
            {
                nNumExpectedWorkers = 2;
            }
            ulong num = (ulong) (nToExclusive - nFromInclusive);
            ulong num2 = num / ((long) nNumExpectedWorkers);
            num2 -= num2 % ((ulong) nStep);
            if (num2 == 0L)
            {
                num2 = (ulong) nStep;
            }
            int num3 = (int) (num / num2);
            if ((num % num2) != 0L)
            {
                num3++;
            }
            long num4 = (long) num2;
            this.m_indexRanges = new IndexRange[num3];
            long num5 = nFromInclusive;
            for (int i = 0; i < num3; i++)
            {
                this.m_indexRanges[i].m_nFromInclusive = num5;
                this.m_indexRanges[i].m_nSharedCurrentIndexOffset = null;
                this.m_indexRanges[i].m_bRangeFinished = 0;
                num5 += num4;
                if ((num5 < (num5 - num4)) || (num5 > nToExclusive))
                {
                    num5 = nToExclusive;
                }
                this.m_indexRanges[i].m_nToExclusive = num5;
            }
        }

        internal RangeWorker RegisterNewWorker()
        {
            return new RangeWorker(this.m_indexRanges, (Interlocked.Increment(ref this.m_nCurrentIndexRangeToAssign) - 1) % this.m_indexRanges.Length, this.m_nStep);
        }
    }
}

