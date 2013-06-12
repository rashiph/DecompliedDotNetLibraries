namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Threading;

    public class TraceEventCache
    {
        private System.DateTime dateTime = System.DateTime.MinValue;
        private static int processId;
        private static string processName;
        private string stackTrace;
        private long timeStamp = -1L;

        internal static int GetProcessId()
        {
            InitProcessInfo();
            return processId;
        }

        internal static string GetProcessName()
        {
            InitProcessInfo();
            return processName;
        }

        internal static int GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        private static void InitProcessInfo()
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            if (processName == null)
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    processId = process.Id;
                    processName = process.ProcessName;
                }
            }
        }

        internal Guid ActivityId
        {
            get
            {
                return Trace.CorrelationManager.ActivityId;
            }
        }

        public string Callstack
        {
            get
            {
                if (this.stackTrace == null)
                {
                    this.stackTrace = Environment.StackTrace;
                }
                else
                {
                    new EnvironmentPermission(PermissionState.Unrestricted).Demand();
                }
                return this.stackTrace;
            }
        }

        public System.DateTime DateTime
        {
            get
            {
                if (this.dateTime == System.DateTime.MinValue)
                {
                    this.dateTime = System.DateTime.UtcNow;
                }
                return this.dateTime;
            }
        }

        public Stack LogicalOperationStack
        {
            get
            {
                return Trace.CorrelationManager.LogicalOperationStack;
            }
        }

        public int ProcessId
        {
            get
            {
                return GetProcessId();
            }
        }

        public string ThreadId
        {
            get
            {
                return GetThreadId().ToString(CultureInfo.InvariantCulture);
            }
        }

        public long Timestamp
        {
            get
            {
                if (this.timeStamp == -1L)
                {
                    this.timeStamp = Stopwatch.GetTimestamp();
                }
                return this.timeStamp;
            }
        }
    }
}

