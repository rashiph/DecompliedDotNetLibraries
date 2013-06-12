namespace System.Diagnostics
{
    using System;

    internal class ProcessData
    {
        public int ProcessId;
        public long StartupTime;

        public ProcessData(int pid, long startTime)
        {
            this.ProcessId = pid;
            this.StartupTime = startTime;
        }
    }
}

