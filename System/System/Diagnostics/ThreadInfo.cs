namespace System.Diagnostics
{
    using System;

    internal class ThreadInfo
    {
        public int basePriority;
        public int currentPriority;
        public int processId;
        public IntPtr startAddress;
        public int threadId;
        public ThreadState threadState;
        public ThreadWaitReason threadWaitReason;
    }
}

