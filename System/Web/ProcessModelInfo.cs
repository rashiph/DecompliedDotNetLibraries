namespace System.Web
{
    using System;
    using System.Security.Permissions;
    using System.Web.Hosting;

    public class ProcessModelInfo
    {
        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        public static ProcessInfo GetCurrentProcessInfo()
        {
            HttpContext current = HttpContext.Current;
            if (((current == null) || (current.WorkerRequest == null)) || !(current.WorkerRequest is ISAPIWorkerRequestOutOfProc))
            {
                throw new HttpException(System.Web.SR.GetString("Process_information_not_available"));
            }
            int dwReqExecuted = 0;
            int dwReqExecuting = 0;
            long tmCreateTime = 0L;
            int pid = 0;
            int dwPeakMemoryUsed = 0;
            if (System.Web.UnsafeNativeMethods.PMGetCurrentProcessInfo(ref dwReqExecuted, ref dwReqExecuting, ref dwPeakMemoryUsed, ref tmCreateTime, ref pid) < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Process_information_not_available"));
            }
            DateTime startTime = DateTime.FromFileTime(tmCreateTime);
            return new ProcessInfo(startTime, DateTime.Now.Subtract(startTime), pid, dwReqExecuted, ProcessStatus.Alive, ProcessShutdownReason.None, dwPeakMemoryUsed);
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.High)]
        public static ProcessInfo[] GetHistory(int numRecords)
        {
            HttpContext current = HttpContext.Current;
            if (((current == null) || (current.WorkerRequest == null)) || !(current.WorkerRequest is ISAPIWorkerRequestOutOfProc))
            {
                throw new HttpException(System.Web.SR.GetString("Process_information_not_available"));
            }
            if (numRecords < 1)
            {
                return null;
            }
            int[] dwPIDArr = new int[numRecords];
            int[] dwReqExecuted = new int[numRecords];
            int[] dwReqExecuting = new int[numRecords];
            int[] dwReqPending = new int[numRecords];
            int[] dwReasonForDeath = new int[numRecords];
            long[] tmCreateTime = new long[numRecords];
            long[] tmDeathTime = new long[numRecords];
            int[] dwPeakMemoryUsed = new int[numRecords];
            int num = System.Web.UnsafeNativeMethods.PMGetHistoryTable(numRecords, dwPIDArr, dwReqExecuted, dwReqPending, dwReqExecuting, dwReasonForDeath, dwPeakMemoryUsed, tmCreateTime, tmDeathTime);
            if (num < 0)
            {
                throw new HttpException(System.Web.SR.GetString("Process_information_not_available"));
            }
            ProcessInfo[] infoArray = new ProcessInfo[num];
            for (int i = 0; i < num; i++)
            {
                DateTime time = DateTime.FromFileTime(tmCreateTime[i]);
                TimeSpan age = DateTime.Now.Subtract(time);
                ProcessStatus alive = ProcessStatus.Alive;
                ProcessShutdownReason none = ProcessShutdownReason.None;
                if (dwReasonForDeath[i] != 0)
                {
                    if (tmDeathTime[i] > 0L)
                    {
                        age = DateTime.FromFileTime(tmDeathTime[i]).Subtract(time);
                    }
                    if ((dwReasonForDeath[i] & 4) != 0)
                    {
                        alive = ProcessStatus.Terminated;
                    }
                    else if ((dwReasonForDeath[i] & 2) != 0)
                    {
                        alive = ProcessStatus.ShutDown;
                    }
                    else
                    {
                        alive = ProcessStatus.ShuttingDown;
                    }
                    if ((0x40 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.IdleTimeout;
                    }
                    else if ((0x80 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.RequestsLimit;
                    }
                    else if ((0x100 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.RequestQueueLimit;
                    }
                    else if ((0x20 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.Timeout;
                    }
                    else if ((0x200 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.MemoryLimitExceeded;
                    }
                    else if ((0x400 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.PingFailed;
                    }
                    else if ((0x800 & dwReasonForDeath[i]) != 0)
                    {
                        none = ProcessShutdownReason.DeadlockSuspected;
                    }
                    else
                    {
                        none = ProcessShutdownReason.Unexpected;
                    }
                }
                infoArray[i] = new ProcessInfo(time, age, dwPIDArr[i], dwReqExecuted[i], alive, none, dwPeakMemoryUsed[i]);
            }
            return infoArray;
        }
    }
}

