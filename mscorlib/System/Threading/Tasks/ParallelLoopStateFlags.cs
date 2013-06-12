namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ParallelLoopStateFlags
    {
        private volatile int m_LoopStateFlags = PLS_NONE;
        internal static int PLS_BROKEN = 2;
        internal static int PLS_CANCELED = 8;
        internal static int PLS_EXCEPTIONAL = 1;
        internal static int PLS_NONE;
        internal static int PLS_STOPPED = 4;

        internal bool AtomicLoopStateUpdate(int newState, int illegalStates)
        {
            int oldState = 0;
            return this.AtomicLoopStateUpdate(newState, illegalStates, ref oldState);
        }

        internal bool AtomicLoopStateUpdate(int newState, int illegalStates, ref int oldState)
        {
            SpinWait wait = new SpinWait();
            while (true)
            {
                oldState = this.m_LoopStateFlags;
                if ((oldState & illegalStates) != 0)
                {
                    return false;
                }
                if (Interlocked.CompareExchange(ref this.m_LoopStateFlags, oldState | newState, oldState) == oldState)
                {
                    return true;
                }
                wait.SpinOnce();
            }
        }

        internal bool Cancel()
        {
            return this.AtomicLoopStateUpdate(PLS_CANCELED, PLS_NONE);
        }

        internal void SetExceptional()
        {
            this.AtomicLoopStateUpdate(PLS_EXCEPTIONAL, PLS_NONE);
        }

        internal void Stop()
        {
            if (!this.AtomicLoopStateUpdate(PLS_STOPPED, PLS_BROKEN))
            {
                throw new InvalidOperationException(Environment.GetResourceString("ParallelState_Stop_InvalidOperationException_StopAfterBreak"));
            }
        }

        internal int LoopStateFlags
        {
            get
            {
                return this.m_LoopStateFlags;
            }
        }
    }
}

