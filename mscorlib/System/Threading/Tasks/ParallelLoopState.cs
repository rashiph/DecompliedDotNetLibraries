namespace System.Threading.Tasks
{
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Threading;

    [DebuggerDisplay("ShouldExitCurrentIteration = {ShouldExitCurrentIteration}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ParallelLoopState
    {
        private ParallelLoopStateFlags m_flagsBase;

        internal ParallelLoopState(ParallelLoopStateFlags fbase)
        {
            this.m_flagsBase = fbase;
        }

        public void Break()
        {
            this.InternalBreak();
        }

        internal static void Break(int iteration, ParallelLoopStateFlags32 pflags)
        {
            int oldState = ParallelLoopStateFlags.PLS_NONE;
            if (!pflags.AtomicLoopStateUpdate(ParallelLoopStateFlags.PLS_BROKEN, (ParallelLoopStateFlags.PLS_STOPPED | ParallelLoopStateFlags.PLS_EXCEPTIONAL) | ParallelLoopStateFlags.PLS_CANCELED, ref oldState))
            {
                if ((oldState & ParallelLoopStateFlags.PLS_STOPPED) != 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("ParallelState_Break_InvalidOperationException_BreakAfterStop"));
                }
            }
            else
            {
                int lowestBreakIteration = pflags.m_lowestBreakIteration;
                if (iteration < lowestBreakIteration)
                {
                    SpinWait wait = new SpinWait();
                    while (Interlocked.CompareExchange(ref pflags.m_lowestBreakIteration, iteration, lowestBreakIteration) != lowestBreakIteration)
                    {
                        wait.SpinOnce();
                        lowestBreakIteration = pflags.m_lowestBreakIteration;
                        if (iteration > lowestBreakIteration)
                        {
                            return;
                        }
                    }
                }
            }
        }

        internal static void Break(long iteration, ParallelLoopStateFlags64 pflags)
        {
            int oldState = ParallelLoopStateFlags.PLS_NONE;
            if (!pflags.AtomicLoopStateUpdate(ParallelLoopStateFlags.PLS_BROKEN, (ParallelLoopStateFlags.PLS_STOPPED | ParallelLoopStateFlags.PLS_EXCEPTIONAL) | ParallelLoopStateFlags.PLS_CANCELED, ref oldState))
            {
                if ((oldState & ParallelLoopStateFlags.PLS_STOPPED) != 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("ParallelState_Break_InvalidOperationException_BreakAfterStop"));
                }
            }
            else
            {
                long lowestBreakIteration = pflags.LowestBreakIteration;
                if (iteration < lowestBreakIteration)
                {
                    SpinWait wait = new SpinWait();
                    while (Interlocked.CompareExchange(ref pflags.m_lowestBreakIteration, iteration, lowestBreakIteration) != lowestBreakIteration)
                    {
                        wait.SpinOnce();
                        lowestBreakIteration = pflags.LowestBreakIteration;
                        if (iteration > lowestBreakIteration)
                        {
                            return;
                        }
                    }
                }
            }
        }

        internal virtual void InternalBreak()
        {
            throw new NotSupportedException(Environment.GetResourceString("ParallelState_NotSupportedException_UnsupportedMethod"));
        }

        public void Stop()
        {
            this.m_flagsBase.Stop();
        }

        internal virtual long? InternalLowestBreakIteration
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("ParallelState_NotSupportedException_UnsupportedMethod"));
            }
        }

        internal virtual bool InternalShouldExitCurrentIteration
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("ParallelState_NotSupportedException_UnsupportedMethod"));
            }
        }

        public bool IsExceptional
        {
            get
            {
                return ((this.m_flagsBase.LoopStateFlags & ParallelLoopStateFlags.PLS_EXCEPTIONAL) != 0);
            }
        }

        public bool IsStopped
        {
            get
            {
                return ((this.m_flagsBase.LoopStateFlags & ParallelLoopStateFlags.PLS_STOPPED) != 0);
            }
        }

        public long? LowestBreakIteration
        {
            get
            {
                return this.InternalLowestBreakIteration;
            }
        }

        public bool ShouldExitCurrentIteration
        {
            get
            {
                return this.InternalShouldExitCurrentIteration;
            }
        }
    }
}

