namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [DefaultProperty("StartInfo"), DefaultEvent("Exited"), MonitoringDescription("ProcessDesc"), Designer("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true, Synchronization=true, ExternalProcessMgmt=true, SelfAffectingProcessMgmt=true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class Process : Component
    {
        private bool disposed;
        internal AsyncStreamReader error;
        private StreamReadMode errorStreamReadMode;
        private int exitCode;
        private bool exited;
        private DateTime exitTime;
        private bool haveExitTime;
        private bool haveMainWindow;
        private bool havePriorityBoostEnabled;
        private bool havePriorityClass;
        private bool haveProcessHandle;
        private bool haveProcessId;
        private bool haveProcessorAffinity;
        private bool haveResponding;
        private bool haveWorkingSetLimits;
        private static SafeFileHandle InvalidPipeHandle = new SafeFileHandle(IntPtr.Zero, false);
        private bool isRemoteMachine;
        private int m_processAccess;
        private Microsoft.Win32.SafeHandles.SafeProcessHandle m_processHandle;
        private string machineName;
        private IntPtr mainWindowHandle;
        private string mainWindowTitle;
        private IntPtr maxWorkingSet;
        private IntPtr minWorkingSet;
        private ProcessModuleCollection modules;
        private System.OperatingSystem operatingSystem;
        internal AsyncStreamReader output;
        private StreamReadMode outputStreamReadMode;
        internal bool pendingErrorRead;
        internal bool pendingOutputRead;
        private bool priorityBoostEnabled;
        private ProcessPriorityClass priorityClass;
        private int processId;
        private ProcessInfo processInfo;
        private IntPtr processorAffinity;
        internal static TraceSwitch processTracing = null;
        private bool raisedOnExited;
        private RegisteredWaitHandle registeredWaitHandle;
        private bool responding;
        private static object s_CreateProcessLock = new object();
        private bool signaled;
        private StreamReader standardError;
        private StreamWriter standardInput;
        private StreamReader standardOutput;
        private ProcessStartInfo startInfo;
        private ISynchronizeInvoke synchronizingObject;
        private ProcessThreadCollection threads;
        private WaitHandle waitHandle;
        private bool watchForExit;
        private bool watchingForExit;

        [MonitoringDescription("ProcessAssociated"), Browsable(true)]
        public event DataReceivedEventHandler ErrorDataReceived;

        [MonitoringDescription("ProcessExited"), Category("Behavior")]
        public event EventHandler Exited;

        [MonitoringDescription("ProcessAssociated"), Browsable(true)]
        public event DataReceivedEventHandler OutputDataReceived;

        public Process()
        {
            this.machineName = ".";
            this.outputStreamReadMode = StreamReadMode.undefined;
            this.errorStreamReadMode = StreamReadMode.undefined;
            this.m_processAccess = 0x1f0fff;
        }

        private Process(string machineName, bool isRemoteMachine, int processId, ProcessInfo processInfo)
        {
            this.processInfo = processInfo;
            this.machineName = machineName;
            this.isRemoteMachine = isRemoteMachine;
            this.processId = processId;
            this.haveProcessId = true;
            this.outputStreamReadMode = StreamReadMode.undefined;
            this.errorStreamReadMode = StreamReadMode.undefined;
            this.m_processAccess = 0x1f0fff;
        }

        [ComVisible(false)]
        public void BeginErrorReadLine()
        {
            if (this.errorStreamReadMode == StreamReadMode.undefined)
            {
                this.errorStreamReadMode = StreamReadMode.asyncMode;
            }
            else if (this.errorStreamReadMode != StreamReadMode.asyncMode)
            {
                throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
            }
            if (this.pendingErrorRead)
            {
                throw new InvalidOperationException(SR.GetString("PendingAsyncOperation"));
            }
            this.pendingErrorRead = true;
            if (this.error == null)
            {
                if (this.standardError == null)
                {
                    throw new InvalidOperationException(SR.GetString("CantGetStandardError"));
                }
                Stream baseStream = this.standardError.BaseStream;
                this.error = new AsyncStreamReader(this, baseStream, new UserCallBack(this.ErrorReadNotifyUser), this.standardError.CurrentEncoding);
            }
            this.error.BeginReadLine();
        }

        [ComVisible(false)]
        public void BeginOutputReadLine()
        {
            if (this.outputStreamReadMode == StreamReadMode.undefined)
            {
                this.outputStreamReadMode = StreamReadMode.asyncMode;
            }
            else if (this.outputStreamReadMode != StreamReadMode.asyncMode)
            {
                throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
            }
            if (this.pendingOutputRead)
            {
                throw new InvalidOperationException(SR.GetString("PendingAsyncOperation"));
            }
            this.pendingOutputRead = true;
            if (this.output == null)
            {
                if (this.standardOutput == null)
                {
                    throw new InvalidOperationException(SR.GetString("CantGetStandardOut"));
                }
                Stream baseStream = this.standardOutput.BaseStream;
                this.output = new AsyncStreamReader(this, baseStream, new UserCallBack(this.OutputReadNotifyUser), this.standardOutput.CurrentEncoding);
            }
            this.output.BeginReadLine();
        }

        private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
        {
            StringBuilder builder = new StringBuilder();
            string str = executableFileName.Trim();
            bool flag = str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal);
            if (!flag)
            {
                builder.Append("\"");
            }
            builder.Append(str);
            if (!flag)
            {
                builder.Append("\"");
            }
            if (!string.IsNullOrEmpty(arguments))
            {
                builder.Append(" ");
                builder.Append(arguments);
            }
            return builder;
        }

        [ComVisible(false)]
        public void CancelErrorRead()
        {
            if (this.error == null)
            {
                throw new InvalidOperationException(SR.GetString("NoAsyncOperation"));
            }
            this.error.CancelOperation();
            this.pendingErrorRead = false;
        }

        [ComVisible(false)]
        public void CancelOutputRead()
        {
            if (this.output == null)
            {
                throw new InvalidOperationException(SR.GetString("NoAsyncOperation"));
            }
            this.output.CancelOperation();
            this.pendingOutputRead = false;
        }

        public void Close()
        {
            if (this.Associated)
            {
                if (this.haveProcessHandle)
                {
                    this.StopWatchingForExit();
                    this.m_processHandle.Close();
                    this.m_processHandle = null;
                    this.haveProcessHandle = false;
                }
                this.haveProcessId = false;
                this.isRemoteMachine = false;
                this.machineName = ".";
                this.raisedOnExited = false;
                this.standardOutput = null;
                this.standardInput = null;
                this.standardError = null;
                this.output = null;
                this.error = null;
                this.Refresh();
            }
        }

        public bool CloseMainWindow()
        {
            IntPtr mainWindowHandle = this.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
            {
                return false;
            }
            if ((Microsoft.Win32.NativeMethods.GetWindowLong(new HandleRef(this, mainWindowHandle), -16) & 0x8000000) != 0)
            {
                return false;
            }
            Microsoft.Win32.NativeMethods.PostMessage(new HandleRef(this, mainWindowHandle), 0x10, IntPtr.Zero, IntPtr.Zero);
            return true;
        }

        private void CompletionCallback(object context, bool wasSignaled)
        {
            this.StopWatchingForExit();
            this.RaiseOnExited();
        }

        private void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs)
        {
            Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES lpPipeAttributes = new Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES {
                bInheritHandle = true
            };
            SafeFileHandle hWritePipe = null;
            try
            {
                if (parentInputs)
                {
                    CreatePipeWithSecurityAttributes(out childHandle, out hWritePipe, lpPipeAttributes, 0);
                }
                else
                {
                    CreatePipeWithSecurityAttributes(out hWritePipe, out childHandle, lpPipeAttributes, 0);
                }
                if (!Microsoft.Win32.NativeMethods.DuplicateHandle(new HandleRef(this, Microsoft.Win32.NativeMethods.GetCurrentProcess()), hWritePipe, new HandleRef(this, Microsoft.Win32.NativeMethods.GetCurrentProcess()), out parentHandle, 0, false, 2))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if ((hWritePipe != null) && !hWritePipe.IsInvalid)
                {
                    hWritePipe.Close();
                }
            }
        }

        private static void CreatePipeWithSecurityAttributes(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES lpPipeAttributes, int nSize)
        {
            if ((!Microsoft.Win32.NativeMethods.CreatePipe(out hReadPipe, out hWritePipe, lpPipeAttributes, nSize) || hReadPipe.IsInvalid) || hWritePipe.IsInvalid)
            {
                throw new Win32Exception();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Close();
                }
                this.disposed = true;
                base.Dispose(disposing);
            }
        }

        private void EnsureState(State state)
        {
            if (((state & State.IsWin2k) != ((State) 0)) && ((this.OperatingSystem.Platform != PlatformID.Win32NT) || (this.OperatingSystem.Version.Major < 5)))
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2kRequired"));
            }
            if (((state & State.IsNt) != ((State) 0)) && (this.OperatingSystem.Platform != PlatformID.Win32NT))
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
            if (((state & State.Associated) != ((State) 0)) && !this.Associated)
            {
                throw new InvalidOperationException(SR.GetString("NoAssociatedProcess"));
            }
            if (((state & State.HaveId) != ((State) 0)) && !this.haveProcessId)
            {
                if (!this.haveProcessHandle)
                {
                    this.EnsureState(State.Associated);
                    throw new InvalidOperationException(SR.GetString("ProcessIdRequired"));
                }
                this.SetProcessId(ProcessManager.GetProcessIdFromHandle(this.m_processHandle));
            }
            if (((state & State.IsLocal) != ((State) 0)) && this.isRemoteMachine)
            {
                throw new NotSupportedException(SR.GetString("NotSupportedRemote"));
            }
            if (((state & State.HaveProcessInfo) != ((State) 0)) && (this.processInfo == null))
            {
                if ((state & State.HaveId) == ((State) 0))
                {
                    this.EnsureState(State.HaveId);
                }
                ProcessInfo[] processInfos = ProcessManager.GetProcessInfos(this.machineName);
                for (int i = 0; i < processInfos.Length; i++)
                {
                    if (processInfos[i].processId == this.processId)
                    {
                        this.processInfo = processInfos[i];
                        break;
                    }
                }
                if (this.processInfo == null)
                {
                    throw new InvalidOperationException(SR.GetString("NoProcessInfo"));
                }
            }
            if ((state & State.Exited) != ((State) 0))
            {
                if (!this.HasExited)
                {
                    throw new InvalidOperationException(SR.GetString("WaitTillExit"));
                }
                if (!this.haveProcessHandle)
                {
                    throw new InvalidOperationException(SR.GetString("NoProcessHandle"));
                }
            }
        }

        private void EnsureWatchingForExit()
        {
            if (!this.watchingForExit)
            {
                lock (this)
                {
                    if (!this.watchingForExit)
                    {
                        this.watchingForExit = true;
                        try
                        {
                            this.waitHandle = new ProcessWaitHandle(this.m_processHandle);
                            this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(this.waitHandle, new WaitOrTimerCallback(this.CompletionCallback), null, -1, true);
                        }
                        catch
                        {
                            this.watchingForExit = false;
                            throw;
                        }
                    }
                }
            }
        }

        private void EnsureWorkingSetLimits()
        {
            this.EnsureState(State.IsNt);
            if (!this.haveWorkingSetLimits)
            {
                Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                try
                {
                    IntPtr ptr;
                    IntPtr ptr2;
                    processHandle = this.GetProcessHandle(0x400);
                    if (!Microsoft.Win32.NativeMethods.GetProcessWorkingSetSize(processHandle, out ptr, out ptr2))
                    {
                        throw new Win32Exception();
                    }
                    this.minWorkingSet = ptr;
                    this.maxWorkingSet = ptr2;
                    this.haveWorkingSetLimits = true;
                }
                finally
                {
                    this.ReleaseProcessHandle(processHandle);
                }
            }
        }

        public static void EnterDebugMode()
        {
            if (ProcessManager.IsNt)
            {
                SetPrivilege("SeDebugPrivilege", 2);
            }
        }

        internal void ErrorReadNotifyUser(string data)
        {
            DataReceivedEventHandler errorDataReceived = this.ErrorDataReceived;
            if (errorDataReceived != null)
            {
                DataReceivedEventArgs e = new DataReceivedEventArgs(data);
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.Invoke(errorDataReceived, new object[] { this, e });
                }
                else
                {
                    errorDataReceived(this, e);
                }
            }
        }

        public static Process GetCurrentProcess()
        {
            return new Process(".", false, Microsoft.Win32.NativeMethods.GetCurrentProcessId(), null);
        }

        public static Process GetProcessById(int processId)
        {
            return GetProcessById(processId, ".");
        }

        public static Process GetProcessById(int processId, string machineName)
        {
            if (!ProcessManager.IsProcessRunning(processId, machineName))
            {
                throw new ArgumentException(SR.GetString("MissingProccess", new object[] { processId.ToString(CultureInfo.CurrentCulture) }));
            }
            return new Process(machineName, ProcessManager.IsRemoteMachine(machineName), processId, null);
        }

        public static Process[] GetProcesses()
        {
            return GetProcesses(".");
        }

        public static Process[] GetProcesses(string machineName)
        {
            bool isRemoteMachine = ProcessManager.IsRemoteMachine(machineName);
            ProcessInfo[] processInfos = ProcessManager.GetProcessInfos(machineName);
            Process[] processArray = new Process[processInfos.Length];
            for (int i = 0; i < processInfos.Length; i++)
            {
                ProcessInfo processInfo = processInfos[i];
                processArray[i] = new Process(machineName, isRemoteMachine, processInfo.processId, processInfo);
            }
            return processArray;
        }

        public static Process[] GetProcessesByName(string processName)
        {
            return GetProcessesByName(processName, ".");
        }

        public static Process[] GetProcessesByName(string processName, string machineName)
        {
            if (processName == null)
            {
                processName = string.Empty;
            }
            Process[] processes = GetProcesses(machineName);
            ArrayList list = new ArrayList();
            for (int i = 0; i < processes.Length; i++)
            {
                if (string.Equals(processName, processes[i].ProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(processes[i]);
                }
            }
            Process[] array = new Process[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        private Microsoft.Win32.SafeHandles.SafeProcessHandle GetProcessHandle(int access)
        {
            return this.GetProcessHandle(access, true);
        }

        private Microsoft.Win32.SafeHandles.SafeProcessHandle GetProcessHandle(int access, bool throwIfExited)
        {
            if (this.haveProcessHandle)
            {
                if (throwIfExited)
                {
                    ProcessWaitHandle handle = null;
                    try
                    {
                        handle = new ProcessWaitHandle(this.m_processHandle);
                        if (handle.WaitOne(0, false))
                        {
                            if (this.haveProcessId)
                            {
                                throw new InvalidOperationException(SR.GetString("ProcessHasExited", new object[] { this.processId.ToString(CultureInfo.CurrentCulture) }));
                            }
                            throw new InvalidOperationException(SR.GetString("ProcessHasExitedNoId"));
                        }
                    }
                    finally
                    {
                        if (handle != null)
                        {
                            handle.Close();
                        }
                    }
                }
                return this.m_processHandle;
            }
            this.EnsureState(State.IsLocal | State.HaveId);
            Microsoft.Win32.SafeHandles.SafeProcessHandle invalidHandle = Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle;
            invalidHandle = ProcessManager.OpenProcess(this.processId, access, throwIfExited);
            if ((throwIfExited && ((access & 0x400) != 0)) && (Microsoft.Win32.NativeMethods.GetExitCodeProcess(invalidHandle, out this.exitCode) && (this.exitCode != 0x103)))
            {
                throw new InvalidOperationException(SR.GetString("ProcessHasExited", new object[] { this.processId.ToString(CultureInfo.CurrentCulture) }));
            }
            return invalidHandle;
        }

        private ProcessThreadTimes GetProcessTimes()
        {
            ProcessThreadTimes times = new ProcessThreadTimes();
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
            try
            {
                processHandle = this.GetProcessHandle(0x400, false);
                if (processHandle.IsInvalid)
                {
                    throw new InvalidOperationException(SR.GetString("ProcessHasExited", new object[] { this.processId.ToString(CultureInfo.CurrentCulture) }));
                }
                if (!Microsoft.Win32.NativeMethods.GetProcessTimes(processHandle, out times.create, out times.exit, out times.kernel, out times.user))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                this.ReleaseProcessHandle(processHandle);
            }
            return times;
        }

        public void Kill()
        {
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
            try
            {
                processHandle = this.GetProcessHandle(1);
                if (!Microsoft.Win32.NativeMethods.TerminateProcess(processHandle, -1))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                this.ReleaseProcessHandle(processHandle);
            }
        }

        public static void LeaveDebugMode()
        {
            if (ProcessManager.IsNt)
            {
                SetPrivilege("SeDebugPrivilege", 0);
            }
        }

        protected void OnExited()
        {
            EventHandler onExited = this.onExited;
            if (onExited != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(onExited, new object[] { this, EventArgs.Empty });
                }
                else
                {
                    onExited(this, EventArgs.Empty);
                }
            }
        }

        private Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcessHandle()
        {
            return this.OpenProcessHandle(0x1f0fff);
        }

        private Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcessHandle(int access)
        {
            if (!this.haveProcessHandle)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                this.SetProcessHandle(this.GetProcessHandle(access));
            }
            return this.m_processHandle;
        }

        internal void OutputReadNotifyUser(string data)
        {
            DataReceivedEventHandler outputDataReceived = this.OutputDataReceived;
            if (outputDataReceived != null)
            {
                DataReceivedEventArgs e = new DataReceivedEventArgs(data);
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.Invoke(outputDataReceived, new object[] { this, e });
                }
                else
                {
                    outputDataReceived(this, e);
                }
            }
        }

        private void RaiseOnExited()
        {
            if (!this.raisedOnExited)
            {
                lock (this)
                {
                    if (!this.raisedOnExited)
                    {
                        this.raisedOnExited = true;
                        this.OnExited();
                    }
                }
            }
        }

        public void Refresh()
        {
            this.processInfo = null;
            this.threads = null;
            this.modules = null;
            this.mainWindowTitle = null;
            this.exited = false;
            this.signaled = false;
            this.haveMainWindow = false;
            this.haveWorkingSetLimits = false;
            this.haveProcessorAffinity = false;
            this.havePriorityClass = false;
            this.haveExitTime = false;
            this.haveResponding = false;
            this.havePriorityBoostEnabled = false;
        }

        private void ReleaseProcessHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle handle)
        {
            if ((handle != null) && (!this.haveProcessHandle || (handle != this.m_processHandle)))
            {
                handle.Close();
            }
        }

        private static void SetPrivilege(string privilegeName, int attrib)
        {
            IntPtr zero = IntPtr.Zero;
            Microsoft.Win32.NativeMethods.LUID lpLuid = new Microsoft.Win32.NativeMethods.LUID();
            IntPtr currentProcess = Microsoft.Win32.NativeMethods.GetCurrentProcess();
            if (!Microsoft.Win32.NativeMethods.OpenProcessToken(new HandleRef(null, currentProcess), 0x20, out zero))
            {
                throw new Win32Exception();
            }
            try
            {
                if (!Microsoft.Win32.NativeMethods.LookupPrivilegeValue(null, privilegeName, out lpLuid))
                {
                    throw new Win32Exception();
                }
                Microsoft.Win32.NativeMethods.TokenPrivileges newState = new Microsoft.Win32.NativeMethods.TokenPrivileges {
                    Luid = lpLuid,
                    Attributes = attrib
                };
                Microsoft.Win32.NativeMethods.AdjustTokenPrivileges(new HandleRef(null, zero), false, newState, 0, IntPtr.Zero, IntPtr.Zero);
                if (Marshal.GetLastWin32Error() != 0)
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(null, zero));
            }
        }

        private void SetProcessHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle)
        {
            this.m_processHandle = processHandle;
            this.haveProcessHandle = true;
            if (this.watchForExit)
            {
                this.EnsureWatchingForExit();
            }
        }

        private void SetProcessId(int processId)
        {
            this.processId = processId;
            this.haveProcessId = true;
        }

        private void SetWorkingSetLimits(object newMin, object newMax)
        {
            this.EnsureState(State.IsNt);
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
            try
            {
                IntPtr ptr;
                IntPtr ptr2;
                processHandle = this.GetProcessHandle(0x500);
                if (!Microsoft.Win32.NativeMethods.GetProcessWorkingSetSize(processHandle, out ptr, out ptr2))
                {
                    throw new Win32Exception();
                }
                if (newMin != null)
                {
                    ptr = (IntPtr) newMin;
                }
                if (newMax != null)
                {
                    ptr2 = (IntPtr) newMax;
                }
                if (((long) ptr) > ((long) ptr2))
                {
                    if (newMin != null)
                    {
                        throw new ArgumentException(SR.GetString("BadMinWorkset"));
                    }
                    throw new ArgumentException(SR.GetString("BadMaxWorkset"));
                }
                if (!Microsoft.Win32.NativeMethods.SetProcessWorkingSetSize(processHandle, ptr, ptr2))
                {
                    throw new Win32Exception();
                }
                if (!Microsoft.Win32.NativeMethods.GetProcessWorkingSetSize(processHandle, out ptr, out ptr2))
                {
                    throw new Win32Exception();
                }
                this.minWorkingSet = ptr;
                this.maxWorkingSet = ptr2;
                this.haveWorkingSetLimits = true;
            }
            finally
            {
                this.ReleaseProcessHandle(processHandle);
            }
        }

        public bool Start()
        {
            this.Close();
            ProcessStartInfo startInfo = this.StartInfo;
            if (startInfo.FileName.Length == 0)
            {
                throw new InvalidOperationException(SR.GetString("FileNameMissing"));
            }
            if (startInfo.UseShellExecute)
            {
                return this.StartWithShellExecuteEx(startInfo);
            }
            return this.StartWithCreateProcess(startInfo);
        }

        public static Process Start(ProcessStartInfo startInfo)
        {
            Process process = new Process();
            if (startInfo == null)
            {
                throw new ArgumentNullException("startInfo");
            }
            process.StartInfo = startInfo;
            if (process.Start())
            {
                return process;
            }
            return null;
        }

        public static Process Start(string fileName)
        {
            return Start(new ProcessStartInfo(fileName));
        }

        public static Process Start(string fileName, string arguments)
        {
            return Start(new ProcessStartInfo(fileName, arguments));
        }

        public static Process Start(string fileName, string userName, SecureString password, string domain)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName) {
                UserName = userName,
                Password = password,
                Domain = domain,
                UseShellExecute = false
            };
            return Start(startInfo);
        }

        public static Process Start(string fileName, string arguments, string userName, SecureString password, string domain)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments) {
                UserName = userName,
                Password = password,
                Domain = domain,
                UseShellExecute = false
            };
            return Start(startInfo);
        }

        private bool StartWithCreateProcess(ProcessStartInfo startInfo)
        {
            if ((startInfo.StandardOutputEncoding != null) && !startInfo.RedirectStandardOutput)
            {
                throw new InvalidOperationException(SR.GetString("StandardOutputEncodingNotAllowed"));
            }
            if ((startInfo.StandardErrorEncoding != null) && !startInfo.RedirectStandardError)
            {
                throw new InvalidOperationException(SR.GetString("StandardErrorEncodingNotAllowed"));
            }
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            StringBuilder cmdLine = BuildCommandLine(startInfo.FileName, startInfo.Arguments);
            Microsoft.Win32.NativeMethods.STARTUPINFO lpStartupInfo = new Microsoft.Win32.NativeMethods.STARTUPINFO();
            Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation = new Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION();
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle();
            Microsoft.Win32.SafeHandles.SafeThreadHandle handle2 = new Microsoft.Win32.SafeHandles.SafeThreadHandle();
            int error = 0;
            SafeFileHandle parentHandle = null;
            SafeFileHandle handle4 = null;
            SafeFileHandle handle5 = null;
            GCHandle handle6 = new GCHandle();
            lock (s_CreateProcessLock)
            {
                try
                {
                    bool flag;
                    if ((startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput) || startInfo.RedirectStandardError)
                    {
                        if (startInfo.RedirectStandardInput)
                        {
                            this.CreatePipe(out parentHandle, out lpStartupInfo.hStdInput, true);
                        }
                        else
                        {
                            lpStartupInfo.hStdInput = new SafeFileHandle(Microsoft.Win32.NativeMethods.GetStdHandle(-10), false);
                        }
                        if (startInfo.RedirectStandardOutput)
                        {
                            this.CreatePipe(out handle4, out lpStartupInfo.hStdOutput, false);
                        }
                        else
                        {
                            lpStartupInfo.hStdOutput = new SafeFileHandle(Microsoft.Win32.NativeMethods.GetStdHandle(-11), false);
                        }
                        if (startInfo.RedirectStandardError)
                        {
                            this.CreatePipe(out handle5, out lpStartupInfo.hStdError, false);
                        }
                        else
                        {
                            lpStartupInfo.hStdError = new SafeFileHandle(Microsoft.Win32.NativeMethods.GetStdHandle(-12), false);
                        }
                        lpStartupInfo.dwFlags = 0x100;
                    }
                    int creationFlags = 0;
                    if (startInfo.CreateNoWindow)
                    {
                        creationFlags |= 0x8000000;
                    }
                    IntPtr zero = IntPtr.Zero;
                    if (startInfo.environmentVariables != null)
                    {
                        bool unicode = false;
                        if (ProcessManager.IsNt)
                        {
                            creationFlags |= 0x400;
                            unicode = true;
                        }
                        handle6 = GCHandle.Alloc(EnvironmentBlock.ToByteArray(startInfo.environmentVariables, unicode), GCHandleType.Pinned);
                        zero = handle6.AddrOfPinnedObject();
                    }
                    string workingDirectory = startInfo.WorkingDirectory;
                    if (workingDirectory == string.Empty)
                    {
                        workingDirectory = Environment.CurrentDirectory;
                    }
                    if (startInfo.UserName.Length != 0)
                    {
                        Microsoft.Win32.NativeMethods.LogonFlags logonFlags = 0;
                        if (startInfo.LoadUserProfile)
                        {
                            logonFlags = Microsoft.Win32.NativeMethods.LogonFlags.LOGON_WITH_PROFILE;
                        }
                        IntPtr password = IntPtr.Zero;
                        try
                        {
                            if (startInfo.Password == null)
                            {
                                password = Marshal.StringToCoTaskMemUni(string.Empty);
                            }
                            else
                            {
                                password = Marshal.SecureStringToCoTaskMemUnicode(startInfo.Password);
                            }
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                            }
                            finally
                            {
                                flag = Microsoft.Win32.NativeMethods.CreateProcessWithLogonW(startInfo.UserName, startInfo.Domain, password, logonFlags, null, cmdLine, creationFlags, zero, workingDirectory, lpStartupInfo, lpProcessInformation);
                                if (!flag)
                                {
                                    error = Marshal.GetLastWin32Error();
                                }
                                if ((lpProcessInformation.hProcess != IntPtr.Zero) && (lpProcessInformation.hProcess != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                                {
                                    processHandle.InitialSetHandle(lpProcessInformation.hProcess);
                                }
                                if ((lpProcessInformation.hThread != IntPtr.Zero) && (lpProcessInformation.hThread != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                                {
                                    handle2.InitialSetHandle(lpProcessInformation.hThread);
                                }
                            }
                            if (!flag)
                            {
                                if ((error != 0xc1) && (error != 0xd8))
                                {
                                    throw new Win32Exception(error);
                                }
                                throw new Win32Exception(error, SR.GetString("InvalidApplication"));
                            }
                            goto Label_03E0;
                        }
                        finally
                        {
                            if (password != IntPtr.Zero)
                            {
                                Marshal.ZeroFreeCoTaskMemUnicode(password);
                            }
                        }
                    }
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        flag = Microsoft.Win32.NativeMethods.CreateProcess(null, cmdLine, null, null, true, creationFlags, zero, workingDirectory, lpStartupInfo, lpProcessInformation);
                        if (!flag)
                        {
                            error = Marshal.GetLastWin32Error();
                        }
                        if ((lpProcessInformation.hProcess != IntPtr.Zero) && (lpProcessInformation.hProcess != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                        {
                            processHandle.InitialSetHandle(lpProcessInformation.hProcess);
                        }
                        if ((lpProcessInformation.hThread != IntPtr.Zero) && (lpProcessInformation.hThread != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                        {
                            handle2.InitialSetHandle(lpProcessInformation.hThread);
                        }
                    }
                    if (!flag)
                    {
                        if ((error != 0xc1) && (error != 0xd8))
                        {
                            throw new Win32Exception(error);
                        }
                        throw new Win32Exception(error, SR.GetString("InvalidApplication"));
                    }
                }
                finally
                {
                    if (handle6.IsAllocated)
                    {
                        handle6.Free();
                    }
                    lpStartupInfo.Dispose();
                }
            }
        Label_03E0:
            if (startInfo.RedirectStandardInput)
            {
                this.standardInput = new StreamWriter(new FileStream(parentHandle, FileAccess.Write, 0x1000, false), Console.InputEncoding, 0x1000);
                this.standardInput.AutoFlush = true;
            }
            if (startInfo.RedirectStandardOutput)
            {
                Encoding encoding = (startInfo.StandardOutputEncoding != null) ? startInfo.StandardOutputEncoding : Console.OutputEncoding;
                this.standardOutput = new StreamReader(new FileStream(handle4, FileAccess.Read, 0x1000, false), encoding, true, 0x1000);
            }
            if (startInfo.RedirectStandardError)
            {
                Encoding encoding2 = (startInfo.StandardErrorEncoding != null) ? startInfo.StandardErrorEncoding : Console.OutputEncoding;
                this.standardError = new StreamReader(new FileStream(handle5, FileAccess.Read, 0x1000, false), encoding2, true, 0x1000);
            }
            bool flag3 = false;
            if (!processHandle.IsInvalid)
            {
                this.SetProcessHandle(processHandle);
                this.SetProcessId(lpProcessInformation.dwProcessId);
                handle2.Close();
                flag3 = true;
            }
            return flag3;
        }

        private bool StartWithShellExecuteEx(ProcessStartInfo startInfo)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!string.IsNullOrEmpty(startInfo.UserName) || (startInfo.Password != null))
            {
                throw new InvalidOperationException(SR.GetString("CantStartAsUser"));
            }
            if ((startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput) || startInfo.RedirectStandardError)
            {
                throw new InvalidOperationException(SR.GetString("CantRedirectStreams"));
            }
            if (startInfo.StandardErrorEncoding != null)
            {
                throw new InvalidOperationException(SR.GetString("StandardErrorEncodingNotAllowed"));
            }
            if (startInfo.StandardOutputEncoding != null)
            {
                throw new InvalidOperationException(SR.GetString("StandardOutputEncodingNotAllowed"));
            }
            if (startInfo.environmentVariables != null)
            {
                throw new InvalidOperationException(SR.GetString("CantUseEnvVars"));
            }
            Microsoft.Win32.NativeMethods.ShellExecuteInfo executeInfo = new Microsoft.Win32.NativeMethods.ShellExecuteInfo {
                fMask = 0x40
            };
            if (startInfo.ErrorDialog)
            {
                executeInfo.hwnd = startInfo.ErrorDialogParentHandle;
            }
            else
            {
                executeInfo.fMask |= 0x400;
            }
            switch (startInfo.WindowStyle)
            {
                case ProcessWindowStyle.Hidden:
                    executeInfo.nShow = 0;
                    break;

                case ProcessWindowStyle.Minimized:
                    executeInfo.nShow = 2;
                    break;

                case ProcessWindowStyle.Maximized:
                    executeInfo.nShow = 3;
                    break;

                default:
                    executeInfo.nShow = 1;
                    break;
            }
            try
            {
                if (startInfo.FileName.Length != 0)
                {
                    executeInfo.lpFile = Marshal.StringToHGlobalAuto(startInfo.FileName);
                }
                if (startInfo.Verb.Length != 0)
                {
                    executeInfo.lpVerb = Marshal.StringToHGlobalAuto(startInfo.Verb);
                }
                if (startInfo.Arguments.Length != 0)
                {
                    executeInfo.lpParameters = Marshal.StringToHGlobalAuto(startInfo.Arguments);
                }
                if (startInfo.WorkingDirectory.Length != 0)
                {
                    executeInfo.lpDirectory = Marshal.StringToHGlobalAuto(startInfo.WorkingDirectory);
                }
                executeInfo.fMask |= 0x100;
                ShellExecuteHelper helper = new ShellExecuteHelper(executeInfo);
                if (helper.ShellExecuteOnSTAThread())
                {
                    goto Label_0325;
                }
                int errorCode = helper.ErrorCode;
                if (errorCode != 0)
                {
                    goto Label_0282;
                }
                long hInstApp = (long) executeInfo.hInstApp;
                if (hInstApp <= 8L)
                {
                    if (hInstApp < 2L)
                    {
                        goto Label_0276;
                    }
                    switch (((int) (hInstApp - 2L)))
                    {
                        case 0:
                            errorCode = 2;
                            goto Label_0282;

                        case 1:
                            errorCode = 3;
                            goto Label_0282;

                        case 2:
                        case 4:
                        case 5:
                            goto Label_0276;

                        case 3:
                            errorCode = 5;
                            goto Label_0282;

                        case 6:
                            errorCode = 8;
                            goto Label_0282;
                    }
                }
                if ((hInstApp <= 0x20L) && (hInstApp >= 0x1aL))
                {
                    switch (((int) (hInstApp - 0x1aL)))
                    {
                        case 0:
                            errorCode = 0x20;
                            goto Label_0282;

                        case 2:
                        case 3:
                        case 4:
                            errorCode = 0x484;
                            goto Label_0282;

                        case 5:
                            errorCode = 0x483;
                            goto Label_0282;

                        case 6:
                            errorCode = 0x485;
                            goto Label_0282;
                    }
                }
            Label_0276:
                errorCode = (int) executeInfo.hInstApp;
            Label_0282:
                if ((errorCode != 0xc1) && (errorCode != 0xd8))
                {
                    throw new Win32Exception(errorCode);
                }
                throw new Win32Exception(errorCode, SR.GetString("InvalidApplication"));
            }
            finally
            {
                if (executeInfo.lpFile != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(executeInfo.lpFile);
                }
                if (executeInfo.lpVerb != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(executeInfo.lpVerb);
                }
                if (executeInfo.lpParameters != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(executeInfo.lpParameters);
                }
                if (executeInfo.lpDirectory != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(executeInfo.lpDirectory);
                }
            }
        Label_0325:
            if (executeInfo.hProcess != IntPtr.Zero)
            {
                Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle(executeInfo.hProcess);
                this.SetProcessHandle(processHandle);
                return true;
            }
            return false;
        }

        private void StopWatchingForExit()
        {
            if (this.watchingForExit)
            {
                lock (this)
                {
                    if (this.watchingForExit)
                    {
                        this.watchingForExit = false;
                        this.registeredWaitHandle.Unregister(null);
                        this.waitHandle.Close();
                        this.waitHandle = null;
                        this.registeredWaitHandle = null;
                    }
                }
            }
        }

        public override string ToString()
        {
            if (this.Associated)
            {
                string processName = string.Empty;
                try
                {
                    processName = this.ProcessName;
                }
                catch (PlatformNotSupportedException)
                {
                }
                if (processName.Length != 0)
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", new object[] { base.ToString(), processName });
                }
            }
            return base.ToString();
        }

        public void WaitForExit()
        {
            this.WaitForExit(-1);
        }

        public bool WaitForExit(int milliseconds)
        {
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
            bool flag;
            ProcessWaitHandle handle2 = null;
            try
            {
                processHandle = this.GetProcessHandle(0x100000, false);
                if (processHandle.IsInvalid)
                {
                    flag = true;
                }
                else
                {
                    handle2 = new ProcessWaitHandle(processHandle);
                    if (handle2.WaitOne(milliseconds, false))
                    {
                        flag = true;
                        this.signaled = true;
                    }
                    else
                    {
                        flag = false;
                        this.signaled = false;
                    }
                }
            }
            finally
            {
                if (handle2 != null)
                {
                    handle2.Close();
                }
                if ((this.output != null) && (milliseconds == -1))
                {
                    this.output.WaitUtilEOF();
                }
                if ((this.error != null) && (milliseconds == -1))
                {
                    this.error.WaitUtilEOF();
                }
                this.ReleaseProcessHandle(processHandle);
            }
            if (flag && this.watchForExit)
            {
                this.RaiseOnExited();
            }
            return flag;
        }

        public bool WaitForInputIdle()
        {
            return this.WaitForInputIdle(0x7fffffff);
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
            bool flag;
            try
            {
                processHandle = this.GetProcessHandle(0x100400);
                switch (Microsoft.Win32.NativeMethods.WaitForInputIdle(processHandle, milliseconds))
                {
                    case 0:
                        return true;

                    case 0x102:
                        return false;
                }
                throw new InvalidOperationException(SR.GetString("InputIdleUnkownError"));
            }
            finally
            {
                this.ReleaseProcessHandle(processHandle);
            }
            return flag;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), MonitoringDescription("ProcessAssociated")]
        private bool Associated
        {
            get
            {
                if (!this.haveProcessId)
                {
                    return this.haveProcessHandle;
                }
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessBasePriority")]
        public int BasePriority
        {
            get
            {
                this.EnsureState(State.HaveProcessInfo);
                return this.processInfo.basePriority;
            }
        }

        [MonitoringDescription("ProcessEnableRaisingEvents"), Browsable(false), DefaultValue(false)]
        public bool EnableRaisingEvents
        {
            get
            {
                return this.watchForExit;
            }
            set
            {
                if (value != this.watchForExit)
                {
                    if (this.Associated)
                    {
                        if (value)
                        {
                            this.OpenProcessHandle();
                            this.EnsureWatchingForExit();
                        }
                        else
                        {
                            this.StopWatchingForExit();
                        }
                    }
                    this.watchForExit = value;
                }
            }
        }

        [MonitoringDescription("ProcessExitCode"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ExitCode
        {
            get
            {
                this.EnsureState(State.Exited);
                return this.exitCode;
            }
        }

        [MonitoringDescription("ProcessExitTime"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime ExitTime
        {
            get
            {
                if (!this.haveExitTime)
                {
                    this.EnsureState(State.Exited | State.IsNt);
                    this.exitTime = this.GetProcessTimes().ExitTime;
                    this.haveExitTime = true;
                }
                return this.exitTime;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), MonitoringDescription("ProcessHandle")]
        public IntPtr Handle
        {
            get
            {
                this.EnsureState(State.Associated);
                return this.OpenProcessHandle(this.m_processAccess).DangerousGetHandle();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessHandleCount")]
        public int HandleCount
        {
            get
            {
                this.EnsureState(State.HaveProcessInfo);
                return this.processInfo.handleCount;
            }
        }

        [Browsable(false), MonitoringDescription("ProcessTerminated"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasExited
        {
            get
            {
                if (!this.exited)
                {
                    this.EnsureState(State.Associated);
                    Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                    try
                    {
                        processHandle = this.GetProcessHandle(0x100400, false);
                        if (processHandle.IsInvalid)
                        {
                            this.exited = true;
                        }
                        else
                        {
                            int num;
                            if (Microsoft.Win32.NativeMethods.GetExitCodeProcess(processHandle, out num) && (num != 0x103))
                            {
                                this.exited = true;
                                this.exitCode = num;
                            }
                            else
                            {
                                if (!this.signaled)
                                {
                                    ProcessWaitHandle handle2 = null;
                                    try
                                    {
                                        handle2 = new ProcessWaitHandle(processHandle);
                                        this.signaled = handle2.WaitOne(0, false);
                                    }
                                    finally
                                    {
                                        if (handle2 != null)
                                        {
                                            handle2.Close();
                                        }
                                    }
                                }
                                if (this.signaled)
                                {
                                    if (!Microsoft.Win32.NativeMethods.GetExitCodeProcess(processHandle, out num))
                                    {
                                        throw new Win32Exception();
                                    }
                                    this.exited = true;
                                    this.exitCode = num;
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.ReleaseProcessHandle(processHandle);
                    }
                    if (this.exited)
                    {
                        this.RaiseOnExited();
                    }
                }
                return this.exited;
            }
        }

        [MonitoringDescription("ProcessId"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Id
        {
            get
            {
                this.EnsureState(State.HaveId);
                return this.processId;
            }
        }

        [MonitoringDescription("ProcessMachineName"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MachineName
        {
            get
            {
                this.EnsureState(State.Associated);
                return this.machineName;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), MonitoringDescription("ProcessMainModule")]
        public ProcessModule MainModule
        {
            get
            {
                if (this.OperatingSystem.Platform == PlatformID.Win32NT)
                {
                    this.EnsureState(State.IsLocal | State.HaveId);
                    return new ProcessModule(NtProcessManager.GetFirstModuleInfo(this.processId));
                }
                ProcessModuleCollection modules = this.Modules;
                this.EnsureState(State.HaveProcessInfo);
                foreach (ProcessModule module in modules)
                {
                    if (module.moduleInfo.Id == this.processInfo.mainModuleId)
                    {
                        return module;
                    }
                }
                return null;
            }
        }

        [MonitoringDescription("ProcessMainWindowHandle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr MainWindowHandle
        {
            get
            {
                if (!this.haveMainWindow)
                {
                    this.EnsureState(State.IsLocal | State.HaveId);
                    this.mainWindowHandle = ProcessManager.GetMainWindowHandle(this.processId);
                    if (this.mainWindowHandle != IntPtr.Zero)
                    {
                        this.haveMainWindow = true;
                    }
                    else
                    {
                        this.EnsureState(State.HaveProcessInfo);
                    }
                }
                return this.mainWindowHandle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessMainWindowTitle")]
        public string MainWindowTitle
        {
            get
            {
                if (this.mainWindowTitle == null)
                {
                    IntPtr mainWindowHandle = this.MainWindowHandle;
                    if (mainWindowHandle == IntPtr.Zero)
                    {
                        this.mainWindowTitle = string.Empty;
                    }
                    else
                    {
                        int capacity = Microsoft.Win32.NativeMethods.GetWindowTextLength(new HandleRef(this, mainWindowHandle)) * 2;
                        StringBuilder lpString = new StringBuilder(capacity);
                        Microsoft.Win32.NativeMethods.GetWindowText(new HandleRef(this, mainWindowHandle), lpString, lpString.Capacity);
                        this.mainWindowTitle = lpString.ToString();
                    }
                }
                return this.mainWindowTitle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessMaxWorkingSet")]
        public IntPtr MaxWorkingSet
        {
            get
            {
                this.EnsureWorkingSetLimits();
                return this.maxWorkingSet;
            }
            set
            {
                this.SetWorkingSetLimits(null, value);
            }
        }

        [MonitoringDescription("ProcessMinWorkingSet"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr MinWorkingSet
        {
            get
            {
                this.EnsureWorkingSetLimits();
                return this.minWorkingSet;
            }
            set
            {
                this.SetWorkingSetLimits(value, null);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessModules")]
        public ProcessModuleCollection Modules
        {
            get
            {
                if (this.modules == null)
                {
                    this.EnsureState(State.IsLocal | State.HaveId);
                    ModuleInfo[] moduleInfos = ProcessManager.GetModuleInfos(this.processId);
                    ProcessModule[] processModules = new ProcessModule[moduleInfos.Length];
                    for (int i = 0; i < moduleInfos.Length; i++)
                    {
                        processModules[i] = new ProcessModule(moduleInfos[i]);
                    }
                    ProcessModuleCollection modules = new ProcessModuleCollection(processModules);
                    this.modules = modules;
                }
                return this.modules;
            }
        }

        [MonitoringDescription("ProcessNonpagedSystemMemorySize"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.NonpagedSystemMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int NonpagedSystemMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.poolNonpagedBytes;
            }
        }

        [MonitoringDescription("ProcessNonpagedSystemMemorySize"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ComVisible(false)]
        public long NonpagedSystemMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.poolNonpagedBytes;
            }
        }

        private System.OperatingSystem OperatingSystem
        {
            get
            {
                if (this.operatingSystem == null)
                {
                    this.operatingSystem = Environment.OSVersion;
                }
                return this.operatingSystem;
            }
        }

        [MonitoringDescription("ProcessPagedMemorySize"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PagedMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PagedMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.pageFileBytes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ComVisible(false), MonitoringDescription("ProcessPagedMemorySize")]
        public long PagedMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.pageFileBytes;
            }
        }

        [MonitoringDescription("ProcessPagedSystemMemorySize"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PagedSystemMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PagedSystemMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.poolPagedBytes;
            }
        }

        [MonitoringDescription("ProcessPagedSystemMemorySize"), ComVisible(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long PagedSystemMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.poolPagedBytes;
            }
        }

        [MonitoringDescription("ProcessPeakPagedMemorySize"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakPagedMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PeakPagedMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.pageFileBytesPeak;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ComVisible(false), MonitoringDescription("ProcessPeakPagedMemorySize")]
        public long PeakPagedMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.pageFileBytesPeak;
            }
        }

        [MonitoringDescription("ProcessPeakVirtualMemorySize"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakVirtualMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PeakVirtualMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.virtualBytesPeak;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ComVisible(false), MonitoringDescription("ProcessPeakVirtualMemorySize")]
        public long PeakVirtualMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.virtualBytesPeak;
            }
        }

        [MonitoringDescription("ProcessPeakWorkingSet"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakWorkingSet64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PeakWorkingSet
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.workingSetPeak;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ComVisible(false), MonitoringDescription("ProcessPeakWorkingSet")]
        public long PeakWorkingSet64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.workingSetPeak;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessPriorityBoostEnabled")]
        public bool PriorityBoostEnabled
        {
            get
            {
                this.EnsureState(State.IsNt);
                if (!this.havePriorityBoostEnabled)
                {
                    Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                    try
                    {
                        processHandle = this.GetProcessHandle(0x400);
                        bool disabled = false;
                        if (!Microsoft.Win32.NativeMethods.GetProcessPriorityBoost(processHandle, out disabled))
                        {
                            throw new Win32Exception();
                        }
                        this.priorityBoostEnabled = !disabled;
                        this.havePriorityBoostEnabled = true;
                    }
                    finally
                    {
                        this.ReleaseProcessHandle(processHandle);
                    }
                }
                return this.priorityBoostEnabled;
            }
            set
            {
                this.EnsureState(State.IsNt);
                Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                try
                {
                    processHandle = this.GetProcessHandle(0x200);
                    if (!Microsoft.Win32.NativeMethods.SetProcessPriorityBoost(processHandle, !value))
                    {
                        throw new Win32Exception();
                    }
                    this.priorityBoostEnabled = value;
                    this.havePriorityBoostEnabled = true;
                }
                finally
                {
                    this.ReleaseProcessHandle(processHandle);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessPriorityClass")]
        public ProcessPriorityClass PriorityClass
        {
            get
            {
                if (!this.havePriorityClass)
                {
                    Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                    try
                    {
                        processHandle = this.GetProcessHandle(0x400);
                        int priorityClass = Microsoft.Win32.NativeMethods.GetPriorityClass(processHandle);
                        if (priorityClass == 0)
                        {
                            throw new Win32Exception();
                        }
                        this.priorityClass = (ProcessPriorityClass) priorityClass;
                        this.havePriorityClass = true;
                    }
                    finally
                    {
                        this.ReleaseProcessHandle(processHandle);
                    }
                }
                return this.priorityClass;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ProcessPriorityClass), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ProcessPriorityClass));
                }
                if (((value & (ProcessPriorityClass.AboveNormal | ProcessPriorityClass.BelowNormal)) != ((ProcessPriorityClass) 0)) && ((this.OperatingSystem.Platform != PlatformID.Win32NT) || (this.OperatingSystem.Version.Major < 5)))
                {
                    throw new PlatformNotSupportedException(SR.GetString("PriorityClassNotSupported"), null);
                }
                Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                try
                {
                    processHandle = this.GetProcessHandle(0x200);
                    if (!Microsoft.Win32.NativeMethods.SetPriorityClass(processHandle, (int) value))
                    {
                        throw new Win32Exception();
                    }
                    this.priorityClass = value;
                    this.havePriorityClass = true;
                }
                finally
                {
                    this.ReleaseProcessHandle(processHandle);
                }
            }
        }

        [MonitoringDescription("ProcessPrivateMemorySize"), Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PrivateMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PrivateMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.privateBytes;
            }
        }

        [ComVisible(false), MonitoringDescription("ProcessPrivateMemorySize"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long PrivateMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.privateBytes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessPrivilegedProcessorTime")]
        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetProcessTimes().PrivilegedProcessorTime;
            }
        }

        [MonitoringDescription("ProcessProcessName"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProcessName
        {
            get
            {
                this.EnsureState(State.HaveProcessInfo);
                if (((this.processInfo.processName.Length == 15) && ProcessManager.IsNt) && (ProcessManager.IsOSOlderThanXP && !this.isRemoteMachine))
                {
                    try
                    {
                        string moduleName = this.MainModule.ModuleName;
                        if (moduleName != null)
                        {
                            this.processInfo.processName = Path.ChangeExtension(Path.GetFileName(moduleName), null);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return this.processInfo.processName;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessProcessorAffinity")]
        public IntPtr ProcessorAffinity
        {
            get
            {
                if (!this.haveProcessorAffinity)
                {
                    Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                    try
                    {
                        IntPtr ptr;
                        IntPtr ptr2;
                        processHandle = this.GetProcessHandle(0x400);
                        if (!Microsoft.Win32.NativeMethods.GetProcessAffinityMask(processHandle, out ptr, out ptr2))
                        {
                            throw new Win32Exception();
                        }
                        this.processorAffinity = ptr;
                    }
                    finally
                    {
                        this.ReleaseProcessHandle(processHandle);
                    }
                    this.haveProcessorAffinity = true;
                }
                return this.processorAffinity;
            }
            set
            {
                Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = null;
                try
                {
                    processHandle = this.GetProcessHandle(0x200);
                    if (!Microsoft.Win32.NativeMethods.SetProcessAffinityMask(processHandle, value))
                    {
                        throw new Win32Exception();
                    }
                    this.processorAffinity = value;
                    this.haveProcessorAffinity = true;
                }
                finally
                {
                    this.ReleaseProcessHandle(processHandle);
                }
            }
        }

        [MonitoringDescription("ProcessResponding"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Responding
        {
            get
            {
                if (!this.haveResponding)
                {
                    IntPtr mainWindowHandle = this.MainWindowHandle;
                    if (mainWindowHandle == IntPtr.Zero)
                    {
                        this.responding = true;
                    }
                    else
                    {
                        IntPtr ptr2;
                        this.responding = Microsoft.Win32.NativeMethods.SendMessageTimeout(new HandleRef(this, mainWindowHandle), 0, IntPtr.Zero, IntPtr.Zero, 2, 0x1388, out ptr2) != IntPtr.Zero;
                    }
                }
                return this.responding;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessSessionId")]
        public int SessionId
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.sessionId;
            }
        }

        [MonitoringDescription("ProcessStandardError"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StreamReader StandardError
        {
            get
            {
                if (this.standardError == null)
                {
                    throw new InvalidOperationException(SR.GetString("CantGetStandardError"));
                }
                if (this.errorStreamReadMode == StreamReadMode.undefined)
                {
                    this.errorStreamReadMode = StreamReadMode.syncMode;
                }
                else if (this.errorStreamReadMode != StreamReadMode.syncMode)
                {
                    throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
                }
                return this.standardError;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), MonitoringDescription("ProcessStandardInput")]
        public StreamWriter StandardInput
        {
            get
            {
                if (this.standardInput == null)
                {
                    throw new InvalidOperationException(SR.GetString("CantGetStandardIn"));
                }
                return this.standardInput;
            }
        }

        [Browsable(false), MonitoringDescription("ProcessStandardOutput"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StreamReader StandardOutput
        {
            get
            {
                if (this.standardOutput == null)
                {
                    throw new InvalidOperationException(SR.GetString("CantGetStandardOut"));
                }
                if (this.outputStreamReadMode == StreamReadMode.undefined)
                {
                    this.outputStreamReadMode = StreamReadMode.syncMode;
                }
                else if (this.outputStreamReadMode != StreamReadMode.syncMode)
                {
                    throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
                }
                return this.standardOutput;
            }
        }

        [MonitoringDescription("ProcessStartInfo"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ProcessStartInfo StartInfo
        {
            get
            {
                if (this.startInfo == null)
                {
                    this.startInfo = new ProcessStartInfo(this);
                }
                return this.startInfo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.startInfo = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessStartTime")]
        public DateTime StartTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetProcessTimes().StartTime;
            }
        }

        [DefaultValue((string) null), Browsable(false), MonitoringDescription("ProcessSynchronizingObject")]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if ((this.synchronizingObject == null) && base.DesignMode)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        object rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is ISynchronizeInvoke))
                        {
                            this.synchronizingObject = (ISynchronizeInvoke) rootComponent;
                        }
                    }
                }
                return this.synchronizingObject;
            }
            set
            {
                this.synchronizingObject = value;
            }
        }

        [MonitoringDescription("ProcessThreads"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProcessThreadCollection Threads
        {
            get
            {
                if (this.threads == null)
                {
                    this.EnsureState(State.HaveProcessInfo);
                    int count = this.processInfo.threadInfoList.Count;
                    ProcessThread[] processThreads = new ProcessThread[count];
                    for (int i = 0; i < count; i++)
                    {
                        processThreads[i] = new ProcessThread(this.isRemoteMachine, (ThreadInfo) this.processInfo.threadInfoList[i]);
                    }
                    ProcessThreadCollection threads = new ProcessThreadCollection(processThreads);
                    this.threads = threads;
                }
                return this.threads;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessTotalProcessorTime")]
        public TimeSpan TotalProcessorTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetProcessTimes().TotalProcessorTime;
            }
        }

        [MonitoringDescription("ProcessUserProcessorTime"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan UserProcessorTime
        {
            get
            {
                this.EnsureState(State.IsNt);
                return this.GetProcessTimes().UserProcessorTime;
            }
        }

        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.VirtualMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), MonitoringDescription("ProcessVirtualMemorySize"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int VirtualMemorySize
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.virtualBytes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessVirtualMemorySize"), ComVisible(false)]
        public long VirtualMemorySize64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.virtualBytes;
            }
        }

        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.WorkingSet64 instead.  http://go.microsoft.com/fwlink/?linkid=14202"), MonitoringDescription("ProcessWorkingSet"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int WorkingSet
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return (int) this.processInfo.workingSet;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("ProcessWorkingSet"), ComVisible(false)]
        public long WorkingSet64
        {
            get
            {
                this.EnsureState(State.HaveNtProcessInfo);
                return this.processInfo.workingSet;
            }
        }

        private enum State
        {
            Associated = 0x20,
            Exited = 0x10,
            HaveId = 1,
            HaveNtProcessInfo = 12,
            HaveProcessInfo = 8,
            IsLocal = 2,
            IsNt = 4,
            IsWin2k = 0x40
        }

        private enum StreamReadMode
        {
            undefined,
            syncMode,
            asyncMode
        }
    }
}

