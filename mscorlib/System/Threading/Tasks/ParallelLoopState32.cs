namespace System.Threading.Tasks
{
    using System;

    internal class ParallelLoopState32 : ParallelLoopState
    {
        private int m_currentIteration;
        private ParallelLoopStateFlags32 m_sharedParallelStateFlags;

        internal ParallelLoopState32(ParallelLoopStateFlags32 sharedParallelStateFlags) : base(sharedParallelStateFlags)
        {
            this.m_sharedParallelStateFlags = sharedParallelStateFlags;
        }

        internal override void InternalBreak()
        {
            ParallelLoopState.Break(this.CurrentIteration, this.m_sharedParallelStateFlags);
        }

        internal int CurrentIteration
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

