namespace System.ServiceProcess
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [InstallerType(typeof(ServiceProcessInstaller))]
    public class ServiceBase : Component
    {
        private int acceptedCommands = 1;
        private bool autoLog;
        private System.ServiceProcess.NativeMethods.ServiceControlCallback commandCallback;
        private System.ServiceProcess.NativeMethods.ServiceControlCallbackEx commandCallbackEx;
        private bool commandPropsFrozen;
        private bool disposed;
        private System.Diagnostics.EventLog eventLog;
        private IntPtr handleName;
        private bool initialized;
        private bool isServiceHosted;
        private System.ServiceProcess.NativeMethods.ServiceMainCallback mainCallback;
        public const int MaxNameLength = 80;
        private bool nameFrozen;
        private string serviceName;
        private ManualResetEvent startCompletedSignal;
        private System.ServiceProcess.NativeMethods.SERVICE_STATUS status = new System.ServiceProcess.NativeMethods.SERVICE_STATUS();
        private IntPtr statusHandle;

        public ServiceBase()
        {
            this.AutoLog = true;
            this.ServiceName = "";
        }

        private unsafe void DeferredContinue()
        {
            fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
            {
                try
                {
                    this.OnContinue();
                    this.WriteEventLogEntry(Res.GetString("ContinueSuccessful"));
                    this.status.currentState = 4;
                }
                catch (Exception exception)
                {
                    this.status.currentState = 7;
                    this.WriteEventLogEntry(Res.GetString("ContinueFailed", new object[] { exception.ToString() }), EventLogEntryType.Error);
                    throw;
                }
                finally
                {
                    System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                }
            }
        }

        private void DeferredCustomCommand(int command)
        {
            try
            {
                this.OnCustomCommand(command);
                this.WriteEventLogEntry(Res.GetString("CommandSuccessful"));
            }
            catch (Exception exception)
            {
                this.WriteEventLogEntry(Res.GetString("CommandFailed", new object[] { exception.ToString() }), EventLogEntryType.Error);
                throw;
            }
        }

        private unsafe void DeferredPause()
        {
            fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
            {
                try
                {
                    this.OnPause();
                    this.WriteEventLogEntry(Res.GetString("PauseSuccessful"));
                    this.status.currentState = 7;
                }
                catch (Exception exception)
                {
                    this.status.currentState = 4;
                    this.WriteEventLogEntry(Res.GetString("PauseFailed", new object[] { exception.ToString() }), EventLogEntryType.Error);
                    throw;
                }
                finally
                {
                    System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                }
            }
        }

        private void DeferredPowerEvent(int eventType, IntPtr eventData)
        {
            try
            {
                PowerBroadcastStatus powerStatus = (PowerBroadcastStatus) eventType;
                this.OnPowerEvent(powerStatus);
                this.WriteEventLogEntry(Res.GetString("PowerEventOK"));
            }
            catch (Exception exception)
            {
                this.WriteEventLogEntry(Res.GetString("PowerEventFailed", new object[] { exception.ToString() }), EventLogEntryType.Error);
                throw;
            }
        }

        private void DeferredSessionChange(int eventType, IntPtr eventData)
        {
            try
            {
                System.ServiceProcess.NativeMethods.WTSSESSION_NOTIFICATION structure = new System.ServiceProcess.NativeMethods.WTSSESSION_NOTIFICATION();
                Marshal.PtrToStructure(eventData, structure);
                this.OnSessionChange(new SessionChangeDescription((SessionChangeReason) eventType, structure.sessionId));
            }
            catch (Exception exception)
            {
                this.WriteEventLogEntry(Res.GetString("SessionChangeFailed", new object[] { exception.ToString() }), EventLogEntryType.Error);
                throw;
            }
        }

        private unsafe void DeferredShutdown()
        {
            try
            {
                this.OnShutdown();
                this.WriteEventLogEntry(Res.GetString("ShutdownOK"));
                if ((this.status.currentState == 7) || (this.status.currentState == 4))
                {
                    fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
                    {
                        this.status.checkPoint = 0;
                        this.status.waitHint = 0;
                        this.status.currentState = 1;
                        System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                        if (this.isServiceHosted)
                        {
                            try
                            {
                                AppDomain.Unload(AppDomain.CurrentDomain);
                            }
                            catch (CannotUnloadAppDomainException exception)
                            {
                                this.WriteEventLogEntry(Res.GetString("FailedToUnloadAppDomain", new object[] { AppDomain.CurrentDomain.FriendlyName, exception.Message }), EventLogEntryType.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                this.WriteEventLogEntry(Res.GetString("ShutdownFailed", new object[] { exception2.ToString() }), EventLogEntryType.Error);
                throw;
            }
        }

        private unsafe void DeferredStop()
        {
            fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
            {
                int currentState = this.status.currentState;
                this.status.checkPoint = 0;
                this.status.waitHint = 0;
                this.status.currentState = 3;
                System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                try
                {
                    this.OnStop();
                    this.WriteEventLogEntry(Res.GetString("StopSuccessful"));
                    this.status.currentState = 1;
                    System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                    if (this.isServiceHosted)
                    {
                        try
                        {
                            AppDomain.Unload(AppDomain.CurrentDomain);
                        }
                        catch (CannotUnloadAppDomainException exception)
                        {
                            this.WriteEventLogEntry(Res.GetString("FailedToUnloadAppDomain", new object[] { AppDomain.CurrentDomain.FriendlyName, exception.Message }), EventLogEntryType.Error);
                        }
                    }
                }
                catch (Exception exception2)
                {
                    this.status.currentState = currentState;
                    System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                    this.WriteEventLogEntry(Res.GetString("StopFailed", new object[] { exception2.ToString() }), EventLogEntryType.Error);
                    throw;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.handleName != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.handleName);
                this.handleName = IntPtr.Zero;
            }
            this.nameFrozen = false;
            this.commandPropsFrozen = false;
            this.disposed = true;
            base.Dispose(disposing);
        }

        private System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY GetEntry()
        {
            System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY service_table_entry = new System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY();
            this.nameFrozen = true;
            service_table_entry.callback = this.mainCallback;
            service_table_entry.name = this.handleName;
            return service_table_entry;
        }

        private void Initialize(bool multipleServices)
        {
            if (!this.initialized)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (!multipleServices)
                {
                    this.status.serviceType = 0x10;
                }
                else
                {
                    this.status.serviceType = 0x20;
                }
                this.status.currentState = 2;
                this.status.controlsAccepted = 0;
                this.status.win32ExitCode = 0;
                this.status.serviceSpecificExitCode = 0;
                this.status.checkPoint = 0;
                this.status.waitHint = 0;
                this.mainCallback = new System.ServiceProcess.NativeMethods.ServiceMainCallback(this.ServiceMainCallback);
                this.commandCallback = new System.ServiceProcess.NativeMethods.ServiceControlCallback(this.ServiceCommandCallback);
                this.commandCallbackEx = new System.ServiceProcess.NativeMethods.ServiceControlCallbackEx(this.ServiceCommandCallbackEx);
                this.handleName = Marshal.StringToHGlobalUni(this.ServiceName);
                this.initialized = true;
            }
        }

        private static void LateBoundMessageBoxShow(string message, string title)
        {
            int num = 0;
            if (IsRTLResources)
            {
                num |= 0x180000;
            }
            Type type = Type.GetType("System.Windows.Forms.MessageBox, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type enumType = Type.GetType("System.Windows.Forms.MessageBoxButtons, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type type3 = Type.GetType("System.Windows.Forms.MessageBoxIcon, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type type4 = Type.GetType("System.Windows.Forms.MessageBoxDefaultButton, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type type5 = Type.GetType("System.Windows.Forms.MessageBoxOptions, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            type.InvokeMember("Show", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { message, title, Enum.ToObject(enumType, 0), Enum.ToObject(type3, 0), Enum.ToObject(type4, 0), Enum.ToObject(type5, num) }, CultureInfo.InvariantCulture);
        }

        protected virtual void OnContinue()
        {
        }

        protected virtual void OnCustomCommand(int command)
        {
        }

        protected virtual void OnPause()
        {
        }

        protected virtual bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return true;
        }

        protected virtual void OnSessionChange(SessionChangeDescription changeDescription)
        {
        }

        protected virtual void OnShutdown()
        {
        }

        protected virtual void OnStart(string[] args)
        {
        }

        protected virtual void OnStop()
        {
        }

        [ComVisible(false)]
        public unsafe void RequestAdditionalTime(int milliseconds)
        {
            fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
            {
                if (((this.status.currentState != 5) && (this.status.currentState != 2)) && ((this.status.currentState != 3) && (this.status.currentState != 6)))
                {
                    throw new InvalidOperationException(Res.GetString("NotInPendingState"));
                }
                this.status.waitHint = milliseconds;
                this.status.checkPoint++;
                System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
            }
        }

        public static void Run(ServiceBase[] services)
        {
            if ((services == null) || (services.Length == 0))
            {
                throw new ArgumentException(Res.GetString("NoServices"));
            }
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                string message = Res.GetString("CantRunOnWin9x");
                string title = Res.GetString("CantRunOnWin9xTitle");
                LateBoundMessageBoxShow(message, title);
            }
            else
            {
                IntPtr entry = Marshal.AllocHGlobal((IntPtr) ((services.Length + 1) * Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY))));
                System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY[] service_table_entryArray = new System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY[services.Length];
                bool multipleServices = services.Length > 1;
                IntPtr zero = IntPtr.Zero;
                for (int i = 0; i < services.Length; i++)
                {
                    services[i].Initialize(multipleServices);
                    service_table_entryArray[i] = services[i].GetEntry();
                    zero = (IntPtr) (((long) entry) + (Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY)) * i));
                    Marshal.StructureToPtr(service_table_entryArray[i], zero, true);
                }
                System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY structure = new System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY {
                    callback = null,
                    name = IntPtr.Zero
                };
                zero = (IntPtr) (((long) entry) + (Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.SERVICE_TABLE_ENTRY)) * services.Length));
                Marshal.StructureToPtr(structure, zero, true);
                bool flag2 = System.ServiceProcess.NativeMethods.StartServiceCtrlDispatcher(entry);
                string str3 = "";
                if (!flag2)
                {
                    str3 = new Win32Exception().Message;
                    string str4 = Res.GetString("CantStartFromCommandLine");
                    if (Environment.UserInteractive)
                    {
                        string str5 = Res.GetString("CantStartFromCommandLineTitle");
                        LateBoundMessageBoxShow(str4, str5);
                    }
                    else
                    {
                        Console.WriteLine(str4);
                    }
                }
                foreach (ServiceBase base2 in services)
                {
                    base2.Dispose();
                    if (!flag2 && (base2.EventLog.Source.Length != 0))
                    {
                        base2.WriteEventLogEntry(Res.GetString("StartFailed", new object[] { str3 }), EventLogEntryType.Error);
                    }
                }
            }
        }

        public static void Run(ServiceBase service)
        {
            if (service == null)
            {
                throw new ArgumentException(Res.GetString("NoServices"));
            }
            Run(new ServiceBase[] { service });
        }

        private unsafe void ServiceCommandCallback(int command)
        {
            fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
            {
                if (command == 4)
                {
                    System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                }
                else if (((this.status.currentState != 5) && (this.status.currentState != 2)) && ((this.status.currentState != 3) && (this.status.currentState != 6)))
                {
                    switch (command)
                    {
                        case 1:
                        {
                            int currentState = this.status.currentState;
                            if ((this.status.currentState == 7) || (this.status.currentState == 4))
                            {
                                this.status.currentState = 3;
                                System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                                this.status.currentState = currentState;
                                new DeferredHandlerDelegate(this.DeferredStop).BeginInvoke(null, null);
                            }
                            goto Label_01AE;
                        }
                        case 2:
                            if (this.status.currentState == 4)
                            {
                                this.status.currentState = 6;
                                System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                                new DeferredHandlerDelegate(this.DeferredPause).BeginInvoke(null, null);
                            }
                            goto Label_01AE;

                        case 3:
                            if (this.status.currentState == 7)
                            {
                                this.status.currentState = 5;
                                System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                                new DeferredHandlerDelegate(this.DeferredContinue).BeginInvoke(null, null);
                            }
                            goto Label_01AE;

                        case 5:
                            new DeferredHandlerDelegate(this.DeferredShutdown).BeginInvoke(null, null);
                            goto Label_01AE;
                    }
                    new DeferredHandlerDelegateCommand(this.DeferredCustomCommand).BeginInvoke(command, null, null);
                }
            }
        Label_01AE:;
        }

        private int ServiceCommandCallbackEx(int command, int eventType, IntPtr eventData, IntPtr eventContext)
        {
            int num = 0;
            switch (command)
            {
                case 13:
                    new DeferredHandlerDelegateAdvanced(this.DeferredPowerEvent).BeginInvoke(eventType, eventData, null, null);
                    return num;

                case 14:
                    new DeferredHandlerDelegateAdvanced(this.DeferredSessionChange).BeginInvoke(eventType, eventData, null, null);
                    return num;
            }
            this.ServiceCommandCallback(command);
            return num;
        }

        [EditorBrowsable(EditorBrowsableState.Never), ComVisible(false)]
        public unsafe void ServiceMainCallback(int argCount, IntPtr argPointer)
        {
            fixed (System.ServiceProcess.NativeMethods.SERVICE_STATUS* service_statusRef = &this.status)
            {
                string[] state = null;
                if (argCount > 0)
                {
                    char** chPtr = (char**) argPointer.ToPointer();
                    state = new string[argCount - 1];
                    for (int i = 0; i < state.Length; i++)
                    {
                        chPtr++;
                        state[i] = Marshal.PtrToStringUni(*((IntPtr*) chPtr));
                    }
                }
                if (!this.initialized)
                {
                    this.isServiceHosted = true;
                    this.Initialize(true);
                }
                if (Environment.OSVersion.Version.Major >= 5)
                {
                    this.statusHandle = System.ServiceProcess.NativeMethods.RegisterServiceCtrlHandlerEx(this.ServiceName, this.commandCallbackEx, IntPtr.Zero);
                }
                else
                {
                    this.statusHandle = System.ServiceProcess.NativeMethods.RegisterServiceCtrlHandler(this.ServiceName, this.commandCallback);
                }
                this.nameFrozen = true;
                if (this.statusHandle == IntPtr.Zero)
                {
                    string message = new Win32Exception().Message;
                    this.WriteEventLogEntry(Res.GetString("StartFailed", new object[] { message }), EventLogEntryType.Error);
                }
                this.status.controlsAccepted = this.acceptedCommands;
                this.commandPropsFrozen = true;
                if ((this.status.controlsAccepted & 1) != 0)
                {
                    this.status.controlsAccepted |= 4;
                }
                if (Environment.OSVersion.Version.Major < 5)
                {
                    this.status.controlsAccepted &= -65;
                }
                this.status.currentState = 2;
                if (System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef))
                {
                    this.startCompletedSignal = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceQueuedMainCallback), state);
                    this.startCompletedSignal.WaitOne();
                    if (!System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef))
                    {
                        this.WriteEventLogEntry(Res.GetString("StartFailed", new object[] { new Win32Exception().Message }), EventLogEntryType.Error);
                        this.status.currentState = 1;
                        System.ServiceProcess.NativeMethods.SetServiceStatus(this.statusHandle, service_statusRef);
                    }
                }
            }
        }

        private void ServiceQueuedMainCallback(object state)
        {
            string[] args = (string[]) state;
            try
            {
                this.OnStart(args);
                this.WriteEventLogEntry(Res.GetString("StartSuccessful"));
                this.status.checkPoint = 0;
                this.status.waitHint = 0;
                this.status.currentState = 4;
            }
            catch (Exception exception)
            {
                this.WriteEventLogEntry(Res.GetString("StartFailed", new object[] { exception.ToString() }), EventLogEntryType.Error);
                this.status.currentState = 1;
            }
            this.startCompletedSignal.Set();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Stop()
        {
            this.DeferredStop();
        }

        private void WriteEventLogEntry(string message)
        {
            try
            {
                if (this.AutoLog)
                {
                    this.EventLog.WriteEntry(message);
                }
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch
            {
            }
        }

        private void WriteEventLogEntry(string message, EventLogEntryType errorType)
        {
            try
            {
                if (this.AutoLog)
                {
                    this.EventLog.WriteEntry(message, errorType);
                }
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch
            {
            }
        }

        [ServiceProcessDescription("SBAutoLog"), DefaultValue(true)]
        public bool AutoLog
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.autoLog;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.autoLog = value;
            }
        }

        [DefaultValue(false)]
        public bool CanHandlePowerEvent
        {
            get
            {
                return ((this.acceptedCommands & 0x40) != 0);
            }
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(Res.GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 0x40;
                }
                else
                {
                    this.acceptedCommands &= -65;
                }
            }
        }

        [ComVisible(false), DefaultValue(false)]
        public bool CanHandleSessionChangeEvent
        {
            get
            {
                return ((this.acceptedCommands & 0x80) != 0);
            }
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(Res.GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 0x80;
                }
                else
                {
                    this.acceptedCommands &= -129;
                }
            }
        }

        [DefaultValue(false)]
        public bool CanPauseAndContinue
        {
            get
            {
                return ((this.acceptedCommands & 2) != 0);
            }
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(Res.GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 2;
                }
                else
                {
                    this.acceptedCommands &= -3;
                }
            }
        }

        [DefaultValue(false)]
        public bool CanShutdown
        {
            get
            {
                return ((this.acceptedCommands & 4) != 0);
            }
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(Res.GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 4;
                }
                else
                {
                    this.acceptedCommands &= -5;
                }
            }
        }

        [DefaultValue(true)]
        public bool CanStop
        {
            get
            {
                return ((this.acceptedCommands & 1) != 0);
            }
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(Res.GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 1;
                }
                else
                {
                    this.acceptedCommands &= -2;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual System.Diagnostics.EventLog EventLog
        {
            get
            {
                if (this.eventLog == null)
                {
                    this.eventLog = new System.Diagnostics.EventLog();
                    this.eventLog.Source = this.ServiceName;
                    this.eventLog.Log = "Application";
                }
                return this.eventLog;
            }
        }

        [ComVisible(false)]
        public int ExitCode
        {
            get
            {
                return this.status.win32ExitCode;
            }
            set
            {
                this.status.win32ExitCode = value;
            }
        }

        private static bool IsRTLResources
        {
            get
            {
                return (Res.GetString("RTL") != "RTL_False");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IntPtr ServiceHandle
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                return this.statusHandle;
            }
        }

        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ServiceProcessDescription("SBServiceName")]
        public string ServiceName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceName;
            }
            set
            {
                if (this.nameFrozen)
                {
                    throw new InvalidOperationException(Res.GetString("CannotChangeName"));
                }
                if ((value != "") && !ServiceController.ValidServiceName(value))
                {
                    object[] args = new object[] { value, 80.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(Res.GetString("ServiceName", args));
                }
                this.serviceName = value;
            }
        }

        private delegate void DeferredHandlerDelegate();

        private delegate void DeferredHandlerDelegateAdvanced(int eventType, IntPtr eventData);

        private delegate void DeferredHandlerDelegateCommand(int command);
    }
}

