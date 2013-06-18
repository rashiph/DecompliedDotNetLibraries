namespace Microsoft.VisualBasic.ApplicationServices
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.VisualBasic.Devices;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Timers;
    using System.Windows.Forms;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class WindowsFormsApplicationBase : ConsoleApplicationBase
    {
        private const int ATTACH_TIMEOUT = 0x9c4;
        private const string HOST_NAME = "127.0.0.1";
        private WinFormsAppContext m_AppContext;
        private SynchronizationContext m_AppSyncronizationContext;
        private bool m_DidSplashScreen;
        private bool m_EnableVisualStyles;
        private bool m_FinishedOnInitilaize;
        [SecurityCritical]
        private SafeFileHandle m_FirstInstanceMemoryMappedFileHandle;
        private EventWaitHandle m_FirstInstanceSemaphore;
        private bool m_IsSingleInstance;
        private string m_MemoryMappedID;
        private EventWaitHandle m_MessageRecievedSemaphore;
        private int m_MinimumSplashExposure;
        private ArrayList m_NetworkAvailabilityEventHandlers;
        private object m_NetworkAvailChangeLock;
        private Network m_NetworkObject;
        private bool m_Ok2CloseSplashScreen;
        private bool m_ProcessingUnhandledExceptionEvent;
        private bool m_SaveMySettingsOnExit;
        private ShutdownMode m_ShutdownStyle;
        private object m_SplashLock;
        private Form m_SplashScreen;
        private System.Timers.Timer m_SplashTimer;
        private SendOrPostCallback m_StartNextInstanceCallback;
        private bool m_TurnOnNetworkListener;
        private ArrayList m_UnhandledExceptionHandlers;
        private const int SECOND_INSTANCE_TIMEOUT = 0x9c4;

        public event NetworkAvailableEventHandler NetworkAvailabilityChanged
        {
            add
            {
                object networkAvailChangeLock = this.m_NetworkAvailChangeLock;
                ObjectFlowControl.CheckForSyncLockOnValueType(networkAvailChangeLock);
                lock (networkAvailChangeLock)
                {
                    if (this.m_NetworkAvailabilityEventHandlers == null)
                    {
                        this.m_NetworkAvailabilityEventHandlers = new ArrayList();
                    }
                    this.m_NetworkAvailabilityEventHandlers.Add(value);
                    this.m_TurnOnNetworkListener = true;
                    if ((this.m_NetworkObject == null) & this.m_FinishedOnInitilaize)
                    {
                        this.m_NetworkObject = new Network();
                        this.m_NetworkObject.NetworkAvailabilityChanged += new NetworkAvailableEventHandler(this.NetworkAvailableEventAdaptor);
                    }
                }
            }
            remove
            {
                if ((this.m_NetworkAvailabilityEventHandlers != null) && (this.m_NetworkAvailabilityEventHandlers.Count > 0))
                {
                    this.m_NetworkAvailabilityEventHandlers.Remove(value);
                    if (this.m_NetworkAvailabilityEventHandlers.Count == 0)
                    {
                        this.m_NetworkObject.NetworkAvailabilityChanged -= new NetworkAvailableEventHandler(this.NetworkAvailableEventAdaptor);
                        if (this.m_NetworkObject != null)
                        {
                            this.m_NetworkObject.DisconnectListener();
                            this.m_NetworkObject = null;
                        }
                    }
                }
            }
            raise
            {
                if (this.m_NetworkAvailabilityEventHandlers != null)
                {
                    IEnumerator enumerator;
                    try
                    {
                        enumerator = this.m_NetworkAvailabilityEventHandlers.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            NetworkAvailableEventHandler current = (NetworkAvailableEventHandler) enumerator.Current;
                            try
                            {
                                if (current != null)
                                {
                                    current(sender, e);
                                }
                                continue;
                            }
                            catch (Exception exception)
                            {
                                if (!this.OnUnhandledException(new Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs(true, exception)))
                                {
                                    throw;
                                }
                                continue;
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            (enumerator as IDisposable).Dispose();
                        }
                    }
                }
            }
        }

        public event ShutdownEventHandler Shutdown;

        public event StartupEventHandler Startup;

        public event StartupNextInstanceEventHandler StartupNextInstance;

        public event Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventHandler UnhandledException
        {
            add
            {
                if (this.m_UnhandledExceptionHandlers == null)
                {
                    this.m_UnhandledExceptionHandlers = new ArrayList();
                }
                this.m_UnhandledExceptionHandlers.Add(value);
                if (this.m_UnhandledExceptionHandlers.Count == 1)
                {
                    Application.ThreadException += new ThreadExceptionEventHandler(this.OnUnhandledExceptionEventAdaptor);
                }
            }
            remove
            {
                if ((this.m_UnhandledExceptionHandlers != null) && (this.m_UnhandledExceptionHandlers.Count > 0))
                {
                    this.m_UnhandledExceptionHandlers.Remove(value);
                    if (this.m_UnhandledExceptionHandlers.Count == 0)
                    {
                        Application.ThreadException -= new ThreadExceptionEventHandler(this.OnUnhandledExceptionEventAdaptor);
                    }
                }
            }
            raise
            {
                if (this.m_UnhandledExceptionHandlers != null)
                {
                    IEnumerator enumerator;
                    this.m_ProcessingUnhandledExceptionEvent = true;
                    try
                    {
                        enumerator = this.m_UnhandledExceptionHandlers.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventHandler current = (Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventHandler) enumerator.Current;
                            if (current != null)
                            {
                                current(sender, e);
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            (enumerator as IDisposable).Dispose();
                        }
                    }
                    this.m_ProcessingUnhandledExceptionEvent = false;
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WindowsFormsApplicationBase() : this(AuthenticationMode.Windows)
        {
        }

        [SecuritySafeCritical]
        public WindowsFormsApplicationBase(AuthenticationMode authenticationMode)
        {
            this.m_MinimumSplashExposure = 0x7d0;
            this.m_SplashLock = new object();
            this.m_NetworkAvailChangeLock = new object();
            this.m_Ok2CloseSplashScreen = true;
            this.ValidateAuthenticationModeEnumValue(authenticationMode, "authenticationMode");
            if (authenticationMode == AuthenticationMode.Windows)
            {
                try
                {
                    Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                }
                catch (SecurityException)
                {
                }
            }
            this.m_AppContext = new WinFormsAppContext(this);
            new UIPermission(UIPermissionWindow.AllWindows).Assert();
            this.m_AppSyncronizationContext = AsyncOperationManager.SynchronizationContext;
            AsyncOperationManager.SynchronizationContext = new WindowsFormsSynchronizationContext();
            PermissionSet.RevertAssert();
        }

        private void DisplaySplash()
        {
            if (this.m_SplashTimer != null)
            {
                this.m_SplashTimer.Enabled = true;
            }
            Application.Run(this.m_SplashScreen);
        }

        private void DoApplicationModel()
        {
            StartupEventArgs eventArgs = new StartupEventArgs(base.CommandLineArgs);
            if (!Debugger.IsAttached)
            {
                try
                {
                    if (this.OnInitialize(base.CommandLineArgs) && this.OnStartup(eventArgs))
                    {
                        this.OnRun();
                        this.OnShutdown();
                    }
                }
                catch (Exception exception)
                {
                    if (this.m_ProcessingUnhandledExceptionEvent)
                    {
                        throw;
                    }
                    if (!this.OnUnhandledException(new Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs(true, exception)))
                    {
                        throw;
                    }
                }
            }
            else if (this.OnInitialize(base.CommandLineArgs) && this.OnStartup(eventArgs))
            {
                this.OnRun();
                this.OnShutdown();
            }
        }

        public void DoEvents()
        {
            Application.DoEvents();
        }

        [SecurityCritical]
        private string GetApplicationInstanceID(Assembly Entry)
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Assert();
            Guid typeLibGuidForAssembly = Marshal.GetTypeLibGuidForAssembly(Entry);
            string[] strArray = Entry.GetName().Version.ToString().Split(Conversions.ToCharArrayRankOne("."));
            PermissionSet.RevertAssert();
            return (typeLibGuidForAssembly.ToString() + strArray[0] + "." + strArray[1]);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecuritySafeCritical]
        protected void HideSplashScreen()
        {
            object splashLock = this.m_SplashLock;
            ObjectFlowControl.CheckForSyncLockOnValueType(splashLock);
            lock (splashLock)
            {
                if (this.MainForm != null)
                {
                    new UIPermission(UIPermissionWindow.AllWindows).Assert();
                    this.MainForm.Activate();
                    PermissionSet.RevertAssert();
                }
                if ((this.m_SplashScreen != null) && !this.m_SplashScreen.IsDisposed)
                {
                    DisposeDelegate method = new DisposeDelegate(this.m_SplashScreen.Dispose);
                    this.m_SplashScreen.Invoke(method);
                    this.m_SplashScreen = null;
                }
            }
        }

        private void MainFormLoadingDone(object sender, EventArgs e)
        {
            this.MainForm.Load -= new EventHandler(this.MainFormLoadingDone);
            while (!this.m_Ok2CloseSplashScreen)
            {
                this.DoEvents();
            }
            this.HideSplashScreen();
        }

        private void MinimumSplashExposureTimeIsUp(object sender, ElapsedEventArgs e)
        {
            if (this.m_SplashTimer != null)
            {
                this.m_SplashTimer.Dispose();
                this.m_SplashTimer = null;
            }
            this.m_Ok2CloseSplashScreen = true;
        }

        private void NetworkAvailableEventAdaptor(object sender, NetworkAvailableEventArgs e)
        {
            this.raise_NetworkAvailabilityChanged(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnCreateMainForm()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnCreateSplashScreen()
        {
        }

        [STAThread, EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual bool OnInitialize(ReadOnlyCollection<string> commandLineArgs)
        {
            if (this.m_EnableVisualStyles)
            {
                Application.EnableVisualStyles();
            }
            if (!commandLineArgs.Contains("/nosplash") && !this.CommandLineArgs.Contains("-nosplash"))
            {
                this.ShowSplashScreen();
            }
            this.m_FinishedOnInitilaize = true;
            return true;
        }

        [SecuritySafeCritical, EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRun()
        {
            if (this.MainForm == null)
            {
                this.OnCreateMainForm();
                if (this.MainForm == null)
                {
                    throw new NoStartupFormException();
                }
                this.MainForm.Load += new EventHandler(this.MainFormLoadingDone);
            }
            try
            {
                Application.Run(this.m_AppContext);
            }
            finally
            {
                if (this.m_NetworkObject != null)
                {
                    this.m_NetworkObject.DisconnectListener();
                }
                if (this.m_FirstInstanceSemaphore != null)
                {
                    this.m_FirstInstanceSemaphore.Close();
                    this.m_FirstInstanceSemaphore = null;
                }
                AsyncOperationManager.SynchronizationContext = this.m_AppSyncronizationContext;
                this.m_AppSyncronizationContext = null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnShutdown()
        {
            ShutdownEventHandler shutdownEvent = this.ShutdownEvent;
            if (shutdownEvent != null)
            {
                shutdownEvent(this, EventArgs.Empty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual bool OnStartup(StartupEventArgs eventArgs)
        {
            eventArgs.Cancel = false;
            if (this.m_TurnOnNetworkListener & (this.m_NetworkObject == null))
            {
                this.m_NetworkObject = new Network();
                this.m_NetworkObject.NetworkAvailabilityChanged += new NetworkAvailableEventHandler(this.NetworkAvailableEventAdaptor);
            }
            StartupEventHandler startupEvent = this.StartupEvent;
            if (startupEvent != null)
            {
                startupEvent(this, eventArgs);
            }
            return !eventArgs.Cancel;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecuritySafeCritical]
        protected virtual void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            StartupNextInstanceEventHandler startupNextInstanceEvent = this.StartupNextInstanceEvent;
            if (startupNextInstanceEvent != null)
            {
                startupNextInstanceEvent(this, eventArgs);
            }
            new UIPermission(UIPermissionWindow.AllWindows).Assert();
            if (eventArgs.BringToForeground && (this.MainForm != null))
            {
                if (this.MainForm.WindowState == FormWindowState.Minimized)
                {
                    this.MainForm.WindowState = FormWindowState.Normal;
                }
                this.MainForm.Activate();
            }
        }

        private void OnStartupNextInstanceMarshallingAdaptor(object args)
        {
            this.OnStartupNextInstance(new StartupNextInstanceEventArgs((ReadOnlyCollection<string>) args, true));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual bool OnUnhandledException(Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
        {
            if ((this.m_UnhandledExceptionHandlers == null) || (this.m_UnhandledExceptionHandlers.Count <= 0))
            {
                return false;
            }
            this.raise_UnhandledException(this, e);
            if (e.ExitApplication)
            {
                Application.Exit();
            }
            return true;
        }

        private void OnUnhandledExceptionEventAdaptor(object sender, ThreadExceptionEventArgs e)
        {
            this.OnUnhandledException(new Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs(true, e.Exception));
        }

        [SecurityCritical]
        private string ReadUrlFromMemoryMappedFile()
        {
            using (SafeFileHandle handle = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.OpenFileMapping(4, false, this.m_MemoryMappedID))
            {
                if (handle.IsInvalid)
                {
                    return null;
                }
                using (SafeMemoryMappedViewOfFileHandle handle2 = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.MapViewOfFile(handle.DangerousGetHandle(), 4, 0, 0, UIntPtr.Zero))
                {
                    if (handle2.IsInvalid)
                    {
                        throw ExceptionUtils.GetWin32Exception("AppModel_CantGetMemoryMappedFile", new string[0]);
                    }
                    return Marshal.PtrToStringUni(handle2.DangerousGetHandle());
                }
            }
        }

        [SecurityCritical]
        private IChannel RegisterChannel(ChannelType ChannelType, bool ChannelIsSecure)
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.SerializationFormatter | SecurityPermissionFlag.UnmanagedCode));
            set.AddPermission(new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "127.0.0.1", 0));
            set.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME"));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration));
            set.Assert();
            IDictionary properties = new Hashtable(3);
            properties.Add("bindTo", "127.0.0.1");
            properties.Add("port", 0);
            properties.Add("name", string.Empty);
            if (ChannelIsSecure)
            {
                properties.Add("secure", true);
                properties.Add("tokenimpersonationlevel", TokenImpersonationLevel.Impersonation);
                properties.Add("impersonate", true);
            }
            IChannel chnl = null;
            if (ChannelType == WindowsFormsApplicationBase.ChannelType.Server)
            {
                chnl = new TcpServerChannel(properties, null);
            }
            else
            {
                chnl = new TcpClientChannel(properties, null);
            }
            ChannelServices.RegisterChannel(chnl, ChannelIsSecure);
            PermissionSet.RevertAssert();
            return chnl;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public void Run(string[] commandLine)
        {
            base.InternalCommandLine = new ReadOnlyCollection<string>(commandLine);
            if (!this.IsSingleInstance)
            {
                this.DoApplicationModel();
            }
            else
            {
                bool flag2;
                string applicationInstanceID = this.GetApplicationInstanceID(Assembly.GetCallingAssembly());
                this.m_MemoryMappedID = applicationInstanceID + "Map";
                string name = applicationInstanceID + "Event";
                string str3 = applicationInstanceID + "Event2";
                this.m_StartNextInstanceCallback = new SendOrPostCallback(this.OnStartupNextInstanceMarshallingAdaptor);
                new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert();
                string identity = WindowsIdentity.GetCurrent().Name;
                bool channelIsSecure = identity != "";
                CodeAccessPermission.RevertAssert();
                if (channelIsSecure)
                {
                    EventWaitHandleAccessRule rule = new EventWaitHandleAccessRule(identity, EventWaitHandleRights.FullControl, AccessControlType.Allow);
                    EventWaitHandleSecurity eventSecurity = new EventWaitHandleSecurity();
                    new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert();
                    eventSecurity.AddAccessRule(rule);
                    CodeAccessPermission.RevertAssert();
                    this.m_FirstInstanceSemaphore = new EventWaitHandle(false, EventResetMode.ManualReset, name, out flag2, eventSecurity);
                    bool createdNew = false;
                    this.m_MessageRecievedSemaphore = new EventWaitHandle(false, EventResetMode.AutoReset, str3, out createdNew, eventSecurity);
                }
                else
                {
                    this.m_FirstInstanceSemaphore = new EventWaitHandle(false, EventResetMode.ManualReset, name, out flag2);
                    this.m_MessageRecievedSemaphore = new EventWaitHandle(false, EventResetMode.AutoReset, str3);
                }
                if (flag2)
                {
                    try
                    {
                        TcpServerChannel channel = (TcpServerChannel) this.RegisterChannel(ChannelType.Server, channelIsSecure);
                        RemoteCommunicator communicator = new RemoteCommunicator(this, this.m_MessageRecievedSemaphore);
                        string uRI = applicationInstanceID + ".rem";
                        new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Assert();
                        RemotingServices.Marshal(communicator, uRI);
                        CodeAccessPermission.RevertAssert();
                        string uRL = channel.GetUrlsForUri(uRI)[0];
                        this.WriteUrlToMemoryMappedFile(uRL);
                        this.m_FirstInstanceSemaphore.Set();
                        this.DoApplicationModel();
                    }
                    finally
                    {
                        if (this.m_MessageRecievedSemaphore != null)
                        {
                            this.m_MessageRecievedSemaphore.Close();
                        }
                        if (this.m_FirstInstanceSemaphore != null)
                        {
                            this.m_FirstInstanceSemaphore.Close();
                        }
                        if ((this.m_FirstInstanceMemoryMappedFileHandle != null) && !this.m_FirstInstanceMemoryMappedFileHandle.IsInvalid)
                        {
                            this.m_FirstInstanceMemoryMappedFileHandle.Close();
                        }
                    }
                }
                else
                {
                    if (!this.m_FirstInstanceSemaphore.WaitOne(0x9c4, false))
                    {
                        throw new CantStartSingleInstanceException();
                    }
                    this.RegisterChannel(ChannelType.Client, channelIsSecure);
                    string url = this.ReadUrlFromMemoryMappedFile();
                    if (url == null)
                    {
                        throw new CantStartSingleInstanceException();
                    }
                    RemoteCommunicator communicator2 = (RemoteCommunicator) RemotingServices.Connect(typeof(RemoteCommunicator), url);
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    set.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.SerializationFormatter | SecurityPermissionFlag.UnmanagedCode));
                    set.AddPermission(new DnsPermission(PermissionState.Unrestricted));
                    set.AddPermission(new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "127.0.0.1", -1));
                    set.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME"));
                    set.Assert();
                    communicator2.RunNextInstance(base.CommandLineArgs);
                    PermissionSet.RevertAssert();
                    if (!this.m_MessageRecievedSemaphore.WaitOne(0x9c4, false))
                    {
                        throw new CantStartSingleInstanceException();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void ShowSplashScreen()
        {
            if (!this.m_DidSplashScreen)
            {
                this.m_DidSplashScreen = true;
                if (this.m_SplashScreen == null)
                {
                    this.OnCreateSplashScreen();
                }
                if (this.m_SplashScreen != null)
                {
                    if (this.m_MinimumSplashExposure > 0)
                    {
                        this.m_Ok2CloseSplashScreen = false;
                        this.m_SplashTimer = new System.Timers.Timer((double) this.m_MinimumSplashExposure);
                        this.m_SplashTimer.Elapsed += new ElapsedEventHandler(this.MinimumSplashExposureTimeIsUp);
                        this.m_SplashTimer.AutoReset = false;
                    }
                    else
                    {
                        this.m_Ok2CloseSplashScreen = true;
                    }
                    new Thread(new ThreadStart(this.DisplaySplash)).Start();
                }
            }
        }

        private void ValidateAuthenticationModeEnumValue(AuthenticationMode value, string paramName)
        {
            if ((value < AuthenticationMode.Windows) || (value > AuthenticationMode.ApplicationDefined))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(AuthenticationMode));
            }
        }

        private void ValidateShutdownModeEnumValue(ShutdownMode value, string paramName)
        {
            if ((value < ShutdownMode.AfterMainFormCloses) || (value > ShutdownMode.AfterAllFormsClose))
            {
                throw new InvalidEnumArgumentException(paramName, (int) value, typeof(ShutdownMode));
            }
        }

        [SecurityCritical]
        private void WriteUrlToMemoryMappedFile(string URL)
        {
            IntPtr ptr = new IntPtr(-1);
            HandleRef hFile = new HandleRef(null, ptr);
            using (NativeTypes.SECURITY_ATTRIBUTES security_attributes = new NativeTypes.SECURITY_ATTRIBUTES())
            {
                bool flag;
                security_attributes.bInheritHandle = false;
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                    flag = Microsoft.VisualBasic.CompilerServices.NativeMethods.ConvertStringSecurityDescriptorToSecurityDescriptor("D:(A;;GA;;;CO)(A;;GR;;;AU)", 1, ref security_attributes.lpSecurityDescriptor, IntPtr.Zero);
                    CodeAccessPermission.RevertAssert();
                }
                catch (EntryPointNotFoundException)
                {
                    security_attributes.lpSecurityDescriptor = IntPtr.Zero;
                }
                catch (DllNotFoundException)
                {
                    security_attributes.lpSecurityDescriptor = IntPtr.Zero;
                }
                if (!flag)
                {
                    security_attributes.lpSecurityDescriptor = IntPtr.Zero;
                }
                this.m_FirstInstanceMemoryMappedFileHandle = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.CreateFileMapping(hFile, security_attributes, 4, 0, (URL.Length + 1) * 2, this.m_MemoryMappedID);
                if (this.m_FirstInstanceMemoryMappedFileHandle.IsInvalid)
                {
                    throw ExceptionUtils.GetWin32Exception("AppModel_CantGetMemoryMappedFile", new string[0]);
                }
            }
            using (SafeMemoryMappedViewOfFileHandle handle = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.MapViewOfFile(this.m_FirstInstanceMemoryMappedFileHandle.DangerousGetHandle(), 2, 0, 0, UIntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    throw ExceptionUtils.GetWin32Exception("AppModel_CantGetMemoryMappedFile", new string[0]);
                }
                char[] source = URL.ToCharArray();
                Marshal.Copy(source, 0, handle.DangerousGetHandle(), source.Length);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public System.Windows.Forms.ApplicationContext ApplicationContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_AppContext;
            }
        }

        protected bool EnableVisualStyles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_EnableVisualStyles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_EnableVisualStyles = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected bool IsSingleInstance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_IsSingleInstance;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_IsSingleInstance = value;
            }
        }

        protected Form MainForm
        {
            get
            {
                return Interaction.IIf<Form>(this.m_AppContext != null, this.m_AppContext.MainForm, null);
            }
            set
            {
                if (value == null)
                {
                    throw ExceptionUtils.GetArgumentNullException("MainForm", "General_PropertyNothing", new string[] { "MainForm" });
                }
                if (value == this.m_SplashScreen)
                {
                    throw new ArgumentException(Utils.GetResourceString("AppModel_SplashAndMainFormTheSame"));
                }
                this.m_AppContext.MainForm = value;
            }
        }

        public int MinimumSplashScreenDisplayTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_MinimumSplashExposure;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_MinimumSplashExposure = value;
            }
        }

        public FormCollection OpenForms
        {
            get
            {
                return Application.OpenForms;
            }
        }

        private SendOrPostCallback RunNextInstanceDelegate
        {
            get
            {
                return this.m_StartNextInstanceCallback;
            }
        }

        public bool SaveMySettingsOnExit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_SaveMySettingsOnExit;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_SaveMySettingsOnExit = value;
            }
        }

        protected internal ShutdownMode ShutdownStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_ShutdownStyle;
            }
            set
            {
                this.ValidateShutdownModeEnumValue(value, "value");
                this.m_ShutdownStyle = value;
            }
        }

        public Form SplashScreen
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_SplashScreen;
            }
            set
            {
                if ((value != null) && (value == this.m_AppContext.MainForm))
                {
                    throw new ArgumentException(Utils.GetResourceString("AppModel_SplashAndMainFormTheSame"));
                }
                this.m_SplashScreen = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static bool UseCompatibleTextRendering
        {
            get
            {
                return false;
            }
        }

        private enum ChannelType : byte
        {
            Client = 1,
            Server = 0
        }

        private delegate void DisposeDelegate();

        private class RemoteCommunicator : MarshalByRefObject
        {
            private AsyncOperation m_AsyncOp;
            private EventWaitHandle m_ConnectionMadeSemaphore;
            private WindowsIdentity m_OriginalUser;
            private SendOrPostCallback m_StartNextInstanceDelegate;

            [SecurityCritical]
            internal RemoteCommunicator(WindowsFormsApplicationBase appObject, EventWaitHandle ConnectionMadeSemaphore)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert();
                this.m_OriginalUser = WindowsIdentity.GetCurrent();
                CodeAccessPermission.RevertAssert();
                this.m_AsyncOp = AsyncOperationManager.CreateOperation(null);
                this.m_StartNextInstanceDelegate = appObject.RunNextInstanceDelegate;
                this.m_ConnectionMadeSemaphore = ConnectionMadeSemaphore;
            }

            [SecurityCritical]
            public override object InitializeLifetimeService()
            {
                return null;
            }

            [SecuritySafeCritical, OneWay]
            public void RunNextInstance(ReadOnlyCollection<string> Args)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert();
                if (this.m_OriginalUser.User == WindowsIdentity.GetCurrent().User)
                {
                    this.m_ConnectionMadeSemaphore.Set();
                    CodeAccessPermission.RevertAssert();
                    this.m_AsyncOp.Post(this.m_StartNextInstanceDelegate, Args);
                }
            }
        }

        private class WinFormsAppContext : ApplicationContext
        {
            private WindowsFormsApplicationBase m_App;

            public WinFormsAppContext(WindowsFormsApplicationBase App)
            {
                this.m_App = App;
            }

            [SecuritySafeCritical]
            protected override void OnMainFormClosed(object sender, EventArgs e)
            {
                if (this.m_App.ShutdownStyle == ShutdownMode.AfterMainFormCloses)
                {
                    base.OnMainFormClosed(sender, e);
                }
                else
                {
                    new UIPermission(UIPermissionWindow.AllWindows).Assert();
                    FormCollection openForms = Application.OpenForms;
                    PermissionSet.RevertAssert();
                    if (openForms.Count > 0)
                    {
                        this.MainForm = openForms[0];
                    }
                    else
                    {
                        base.OnMainFormClosed(sender, e);
                    }
                }
            }
        }
    }
}

