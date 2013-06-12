namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ParallelLoopResult
    {
        internal bool m_completed;
        internal long? m_lowestBreakIteration;
        public bool IsCompleted
        {
            get
            {
                return this.m_completed;
            }
        }
        public long? LowestBreakIteration
        {
            get
            {
                return this.m_lowestBreakIteration;
            }
        }
    }
}

