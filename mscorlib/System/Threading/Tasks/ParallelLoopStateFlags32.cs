namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ParallelLoopStateFlags32 : ParallelLoopStateFlags
    {
        internal volatile int m_lowestBreakIteration = 0x7fffffff;

        internal bool ShouldExitLoop()
        {
            int loopStateFlags = base.LoopStateFlags;
            return ((loopStateFlags != ParallelLoopStateFlags.PLS_NONE) && ((loopStateFlags & (ParallelLoopStateFlags.PLS_EXCEPTIONAL | ParallelLoopStateFlags.PLS_CANCELED)) != 0));
        }

        internal bool ShouldExitLoop(int CallerIteration)
        {
            int loopStateFlags = base.LoopStateFlags;
            if (loopStateFlags == ParallelLoopStateFlags.PLS_NONE)
            {
                return false;
            }
            return (((loopStateFlags & ((ParallelLoopStateFlags.PLS_EXCEPTIONAL | ParallelLoopStateFlags.PLS_STOPPED) | ParallelLoopStateFlags.PLS_CANCELED)) != 0) || (((loopStateFlags & ParallelLoopStateFlags.PLS_BROKEN) != 0) && (CallerIteration > this.LowestBreakIteration)));
        }

        internal int LowestBreakIteration
        {
            get
            {
                return this.m_lowestBreakIteration;
            }
        }

        internal long? NullableLowestBreakIteration
        {
            get
            {
                if (this.m_lowestBreakIteration == 0x7fffffff)
                {
                    return null;
                }
                long lowestBreakIteration = this.m_lowestBreakIteration;
                if (IntPtr.Size >= 8)
                {
                    return new long?(lowestBreakIteration);
                }
                return new long?(Interlocked.Read(ref lowestBreakIteration));
            }
        }
    }
}

