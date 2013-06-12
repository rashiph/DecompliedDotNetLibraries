namespace System.Threading.Tasks
{
    using System;
    using System.Threading;

    internal class ParallelLoopStateFlags64 : ParallelLoopStateFlags
    {
        internal long m_lowestBreakIteration = 0x7fffffffffffffffL;

        internal bool ShouldExitLoop()
        {
            int loopStateFlags = base.LoopStateFlags;
            return ((loopStateFlags != ParallelLoopStateFlags.PLS_NONE) && ((loopStateFlags & (ParallelLoopStateFlags.PLS_EXCEPTIONAL | ParallelLoopStateFlags.PLS_CANCELED)) != 0));
        }

        internal bool ShouldExitLoop(long CallerIteration)
        {
            int loopStateFlags = base.LoopStateFlags;
            if (loopStateFlags == ParallelLoopStateFlags.PLS_NONE)
            {
                return false;
            }
            return (((loopStateFlags & ((ParallelLoopStateFlags.PLS_EXCEPTIONAL | ParallelLoopStateFlags.PLS_STOPPED) | ParallelLoopStateFlags.PLS_CANCELED)) != 0) || (((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0) && (CallerIteration > this.LowestBreakIteration)));
        }

        internal long LowestBreakIteration
        {
            get
            {
                if (IntPtr.Size >= 8)
                {
                    return this.m_lowestBreakIteration;
                }
                return Interlocked.Read(ref this.m_lowestBreakIteration);
            }
        }

        internal long? NullableLowestBreakIteration
        {
            get
            {
                if (this.m_lowestBreakIteration == 0x7fffffffffffffffL)
                {
                    return null;
                }
                if (IntPtr.Size >= 8)
                {
                    return new long?(this.m_lowestBreakIteration);
                }
                return new long?(Interlocked.Read(ref this.m_lowestBreakIteration));
            }
        }
    }
}

