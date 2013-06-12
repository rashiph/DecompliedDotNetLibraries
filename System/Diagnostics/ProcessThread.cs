namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Designer("System.Diagnostics.Design.ProcessThreadDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), HostProtection(SecurityAction.LinkDemand, SelfAffectingProcessMgmt=true, SelfAffectingThreading=true)]
    public class ProcessThread : Component
    {
        private bool havePriorityBoostEnabled;
        private bool havePriorityLevel;
        private bool isRemoteMachine;
        private bool priorityBoostEnabled;
        private ThreadPriorityLevel priorityLevel;
        private ThreadInfo threadInfo;

        internal ProcessThread(bool isRemoteMachine, ThreadInfo threadInfo)
        {
            this.isRemoteMachine = isRemoteMachine;
            this.threadInfo = threadInfo;
            GC.SuppressFinalize(this);
        }

        private static void CloseThreadHandle(Microsoft.Win32.SafeHandles.SafeThreadHandle handle)
        {
            if (handle != null)
            {
                handle.Close();
            }
        }

        private void EnsureState(State state)
        {
            if (((state & State.IsLocal) != ((State) 0)) && this.isRemoteMachine)
            {
                throw new NotSupportedException(SR.GetString("NotSupportedRemoteThread"));
            }
            if (((state & State.IsNt) != ((State) 0)) && (Environment.OSVersion.Platform != PlatformID.Win32NT))
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
        }

        private ProcessThreadTimes GetThreadTimes()
        {
            ProcessThreadTimes times = new ProcessThreadTimes();
            Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
            try
            {
                handle = this.OpenThreadHandle(0x40);
                if (!Microsoft.Win32.NativeMethods.GetThreadTimes(handle, out times.create, out times.exit, out times.kernel, out times.user))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                CloseThreadHandle(handle);
            }
            return times;
        }

        private Microsoft.Win32.SafeHandles.SafeThreadHandle OpenThreadHandle(int access)
        {
            this.EnsureState(State.IsLocal);
            return ProcessManager.OpenThread(this.threadInfo.threadId, access);
        }

        public void ResetIdealProcessor()
        {
            this.IdealProcessor = 0x20;
        }

        [MonitoringDescription("ThreadBasePriority")]
        public int BasePriority
        {
            get
            {
                return this.threadInfo.basePriority;
            }
        }

        [MonitoringDescription("ThreadCurrentPriority")]
        public int CurrentPriority
        {
            get
            {
                return this.threadInfo.currentPriority;
            }
        }

        [MonitoringDescription("ThreadId")]
        public int Id
        {
            get
            {
                return this.threadInfo.threadId;
            }
        }

        [Browsable(false)]
        public int IdealProcessor
        {
            set
            {
                Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
                try
                {
                    handle = this.OpenThreadHandle(0x20);
                    if (Microsoft.Win32.NativeMethods.SetThreadIdealProcessor(handle, value) < 0)
                    {
                        throw new Win32Exception();
                    }
                }
                finally
                {
                    CloseThreadHandle(handle);
                }
            }
        }

        [MonitoringDescription("ThreadPriorityBoostEnabled")]
        public bool PriorityBoostEnabled
        {
            get
            {
                if (!this.havePriorityBoostEnabled)
                {
                    Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
                    try
                    {
                        handle = this.OpenThreadHandle(0x40);
                        bool disabled = false;
                        if (!Microsoft.Win32.NativeMethods.GetThreadPriorityBoost(handle, out disabled))
                        {
                            throw new Win32Exception();
                        }
                        this.priorityBoostEnabled = !disabled;
                        this.havePriorityBoostEnabled = true;
                    }
                    finally
                    {
                        CloseThreadHandle(handle);
                    }
                }
                return this.priorityBoostEnabled;
            }
            set
            {
                Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
                try
                {
                    handle = this.OpenThreadHandle(0x20);
                    if (!Microsoft.Win32.NativeMethods.SetThreadPriorityBoost(handle, !value))
                    {
                        throw new Win32Exception();
                    }
                    this.priorityBoostEnabled = value;
                    this.havePriorityBoostEnabled = true;
                }
                finally
                {
                    CloseThreadHandle(handle);
                }
            }
        }

        [MonitoringDescription("ThreadPriorityLevel")]
        public ThreadPriorityLevel PriorityLevel
        {
            get
            {
                if (!this.havePriorityLevel)
                {
                    Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
                    try
                    {
                        handle = this.OpenThreadHandle(0x40);
                        int threadPriority = Microsoft.Win32.NativeMethods.GetThreadPriority(handle);
                        if (threadPriority == 0x7fffffff)
                        {
                            throw new Win32Exception();
                        }
                        this.priorityLevel = (ThreadPriorityLevel) threadPriority;
                        this.havePriorityLevel = true;
                    }
                    finally
                    {
                        CloseThreadHandle(handle);
                    }
                }
                return this.priorityLevel;
            }
            set
            {
                Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
                try
                {
                    handle = this.OpenThreadHandle(0x20);
                    if (!Microsoft.Win32.NativeMethods.SetThreadPriority(handle, (int) value))
                    {
                        throw new Win32Exception();
                    }
                    this.priorityLevel = value;
                }
                finally
                {
                    CloseThreadHandle(handle);
                }
            }
        }

        [MonitoringDescription("ThreadPrivilegedProcessorTime")]
        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetThreadTimes().PrivilegedProcessorTime;
            }
        }

        [Browsable(false)]
        public IntPtr ProcessorAffinity
        {
            set
            {
                Microsoft.Win32.SafeHandles.SafeThreadHandle handle = null;
                try
                {
                    handle = this.OpenThreadHandle(0x60);
                    if (Microsoft.Win32.NativeMethods.SetThreadAffinityMask(handle, new HandleRef(this, value)) == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                }
                finally
                {
                    CloseThreadHandle(handle);
                }
            }
        }

        [MonitoringDescription("ThreadStartAddress")]
        public IntPtr StartAddress
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.threadInfo.startAddress;
            }
        }

        [MonitoringDescription("ThreadStartTime")]
        public DateTime StartTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetThreadTimes().StartTime;
            }
        }

        [MonitoringDescription("ThreadThreadState")]
        public System.Diagnostics.ThreadState ThreadState
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.threadInfo.threadState;
            }
        }

        [MonitoringDescription("ThreadTotalProcessorTime")]
        public TimeSpan TotalProcessorTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetThreadTimes().TotalProcessorTime;
            }
        }

        [MonitoringDescription("ThreadUserProcessorTime")]
        public TimeSpan UserProcessorTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetThreadTimes().UserProcessorTime;
            }
        }

        [MonitoringDescription("ThreadWaitReason")]
        public ThreadWaitReason WaitReason
        {
            get
            {
                this.EnsureState(State.IsNt);
                if (this.threadInfo.threadState != System.Diagnostics.ThreadState.Wait)
                {
                    throw new InvalidOperationException(SR.GetString("WaitReasonUnavailable"));
                }
                return this.threadInfo.threadWaitReason;
            }
        }

        private enum State
        {
            IsLocal = 2,
            IsNt = 4
        }
    }
}

