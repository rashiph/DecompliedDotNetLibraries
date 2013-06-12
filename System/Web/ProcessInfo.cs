namespace System.Web
{
    using System;

    public class ProcessInfo
    {
        private TimeSpan _Age;
        private int _PeakMemoryUsed;
        private int _ProcessID;
        private int _RequestCount;
        private ProcessShutdownReason _ShutdownReason;
        private DateTime _StartTime;
        private ProcessStatus _Status;

        public ProcessInfo()
        {
        }

        public ProcessInfo(DateTime startTime, TimeSpan age, int processID, int requestCount, ProcessStatus status, ProcessShutdownReason shutdownReason, int peakMemoryUsed)
        {
            this._StartTime = startTime;
            this._Age = age;
            this._ProcessID = processID;
            this._RequestCount = requestCount;
            this._Status = status;
            this._ShutdownReason = shutdownReason;
            this._PeakMemoryUsed = peakMemoryUsed;
        }

        public void SetAll(DateTime startTime, TimeSpan age, int processID, int requestCount, ProcessStatus status, ProcessShutdownReason shutdownReason, int peakMemoryUsed)
        {
            this._StartTime = startTime;
            this._Age = age;
            this._ProcessID = processID;
            this._RequestCount = requestCount;
            this._Status = status;
            this._ShutdownReason = shutdownReason;
            this._PeakMemoryUsed = peakMemoryUsed;
        }

        public TimeSpan Age
        {
            get
            {
                return this._Age;
            }
        }

        public int PeakMemoryUsed
        {
            get
            {
                return this._PeakMemoryUsed;
            }
        }

        public int ProcessID
        {
            get
            {
                return this._ProcessID;
            }
        }

        public int RequestCount
        {
            get
            {
                return this._RequestCount;
            }
        }

        public ProcessShutdownReason ShutdownReason
        {
            get
            {
                return this._ShutdownReason;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this._StartTime;
            }
        }

        public ProcessStatus Status
        {
            get
            {
                return this._Status;
            }
        }
    }
}

