namespace System.Threading.Tasks
{
    using System;

    internal class ParallelLoopState64 : ParallelLoopState
    {
        private long m_currentIteration;
        private ParallelLoopStateFlags64 m_sharedParallelStateFlags;

        internal ParallelLoopState64(ParallelLoopStateFlags64 sharedParallelStateFlags) : base(sharedParallelStateFlags)
        {
            this.m_sharedParallelStateFlags = sharedParallelStateFlags;
        }

        internal override void InternalBreak()
        {
            ParallelLoopState.Break(this.CurrentIteration, this.m_sharedParallelStateFlags);
        }

        internal long CurrentIteration
        {
            get
            {
                return this.m_currentIteration;
            }
            set
            {
                this.m_currentIteration = value;
            }
        }

        internal override long? InternalLowestBreakIteration
        {
            get
            {
                return this.m_sharedParallelStateFlags.NullableLowestBreakIteration;
            }
        }

        internal override bool InternalShouldExitCurrentIteration
        {
            get
            {
                return this.m_sharedParallelStateFlags.ShouldExitLoop(this.CurrentIteration);
            }
        }
    }
}

