namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Deployment.Internal.Isolation;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    public sealed class Application
    {
        private static object appFileVersion;
        private static bool checkedThreadAffinity = false;
        private const string CLICKONCE_APPS_DATADIRECTORY = "DataDirectory";
        private static string companyName;
        private static readonly object EVENT_APPLICATIONEXIT = new object();
        private static readonly object EVENT_THREADEXIT = new object();
        private static EventHandlerList eventHandlers;
        private const string everettThreadAffinityValue = "EnableSystemEventsThreadAffinityCompatibility";
        private static string executablePath;
        private static bool exiting;
        private static FormCollection forms = null;
        private const string IEEXEC = "ieexec.exe";
        private static object internalSyncObject = new object();
        private static System.Type mainType;
        private static string productName;
        private static string productVersion;
        private static string safeTopLevelCaptionSuffix;
        private static string startupPath;
        private static bool useEverettThreadAffinity = false;
        private static bool useVisualStyles = false;
        private static bool useWaitCursor = false;

        public static  event EventHandler ApplicationExit
        {
            add
            {
                AddEventHandler(EVENT_APPLICATIONEXIT, value);
            }
            remove
            {
                RemoveEventHandler(EVENT_APPLICATIONEXIT, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static  event EventHandler EnterThreadModal
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)] add
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.enterModalHandler = (EventHandler) Delegate.Combine(context.enterModalHandler, value);
                }
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)] remove
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.enterModalHandler = (EventHandler) Delegate.Remove(context.enterModalHandler, value);
                }
            }
        }

        public static  event EventHandler Idle
        {
            add
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.idleHandler = (EventHandler) Delegate.Combine(context.idleHandler, value);
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = context.ComponentManager;
                }
            }
            remove
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.idleHandler = (EventHandler) Delegate.Remove(context.idleHandler, value);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static  event EventHandler LeaveThreadModal
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)] add
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.leaveModalHandler = (EventHandler) Delegate.Combine(context.leaveModalHandler, value);
                }
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)] remove
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.leaveModalHandler = (EventHandler) Delegate.Remove(context.leaveModalHandler, value);
                }
            }
        }

        public static  event ThreadExceptionEventHandler ThreadException
        {
            add
            {
                System.Windows.Forms.IntSecurity.AffectThreadBehavior.Demand();
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.threadExceptionHandler = value;
                }
            }
            remove
            {
                ThreadContext context = ThreadContext.FromCurrent();
                lock (context)
                {
                    context.threadExceptionHandler = (ThreadExceptionEventHandler) Delegate.Remove(context.threadExceptionHandler, value);
                }
            }
        }

        public static  event EventHandler ThreadExit
        {
            add
            {
                AddEventHandler(EVENT_THREADEXIT, value);
            }
            remove
            {
                RemoveEventHandler(EVENT_THREADEXIT, value);
            }
        }

        private Application()
        {
        }

        private static void AddEventHandler(object key, Delegate value)
        {
            lock (internalSyncObject)
            {
                if (eventHandlers == null)
                {
                    eventHandlers = new EventHandlerList();
                }
                eventHandlers.AddHandler(key, value);
            }
        }

        public static void AddMessageFilter(IMessageFilter value)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            ThreadContext.FromCurrent().AddMessageFilter(value);
        }

        internal static void BeginModalMessageLoop()
        {
            ThreadContext.FromCurrent().BeginModalMessageLoop(null);
        }

        public static void DoEvents()
        {
            ThreadContext.FromCurrent().RunMessageLoop(2, null);
        }

        internal static void DoEventsModal()
        {
            ThreadContext.FromCurrent().RunMessageLoop(-2, null);
        }

        public static void EnableVisualStyles()
        {
            string assemblyFileName = null;
            new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
            try
            {
                assemblyFileName = typeof(Application).Assembly.Location;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            if (assemblyFileName != null)
            {
                EnableVisualStylesInternal(assemblyFileName, 0x65);
            }
        }

        private static void EnableVisualStylesInternal(string assemblyFileName, int nativeResourceID)
        {
            useVisualStyles = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.CreateActivationContext(assemblyFileName, nativeResourceID);
        }

        internal static void EndModalMessageLoop()
        {
            ThreadContext.FromCurrent().EndModalMessageLoop(null);
        }

        public static void Exit()
        {
            Exit(null);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Exit(CancelEventArgs e)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            if (((entryAssembly == null) || (callingAssembly == null)) || !entryAssembly.Equals(callingAssembly))
            {
                System.Windows.Forms.IntSecurity.AffectThreadBehavior.Demand();
            }
            bool flag = ExitInternal();
            if (e != null)
            {
                e.Cancel = flag;
            }
        }

        private static bool ExitInternal()
        {
            bool flag = false;
            lock (internalSyncObject)
            {
                if (exiting)
                {
                    return false;
                }
                exiting = true;
                try
                {
                    if (forms != null)
                    {
                        foreach (Form form in OpenFormsInternal)
                        {
                            if (form.RaiseFormClosingOnAppExit())
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        if (forms != null)
                        {
                            while (OpenFormsInternal.Count > 0)
                            {
                                OpenFormsInternal[0].RaiseFormClosedOnAppExit();
                            }
                        }
                        ThreadContext.ExitApplication();
                    }
                    return flag;
                }
                finally
                {
                    exiting = false;
                }
            }
            return flag;
        }

        public static void ExitThread()
        {
            System.Windows.Forms.IntSecurity.AffectThreadBehavior.Demand();
            ExitThreadInternal();
        }

        private static void ExitThreadInternal()
        {
            ThreadContext context = ThreadContext.FromCurrent();
            if (context.ApplicationContext != null)
            {
                context.ApplicationContext.ExitThread();
            }
            else
            {
                context.Dispose(true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static bool FilterMessage(ref Message message)
        {
            bool flag;
            System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG {
                hwnd = message.HWnd,
                message = message.Msg,
                wParam = message.WParam,
                lParam = message.LParam
            };
            bool flag2 = ThreadContext.FromCurrent().ProcessFilters(ref msg, out flag);
            if (flag)
            {
                message.HWnd = msg.hwnd;
                message.Msg = msg.message;
                message.WParam = msg.wParam;
                message.LParam = msg.lParam;
            }
            return flag2;
        }

        internal static void FormActivated(bool modal, bool activated)
        {
            if (!modal)
            {
                ThreadContext.FromCurrent().FormActivated(activated);
            }
        }

        private static FileVersionInfo GetAppFileVersionInfo()
        {
            lock (internalSyncObject)
            {
                if (appFileVersion == null)
                {
                    System.Type appMainType = GetAppMainType();
                    if (appMainType != null)
                    {
                        new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read }.Assert();
                        try
                        {
                            appFileVersion = FileVersionInfo.GetVersionInfo(appMainType.Module.FullyQualifiedName);
                            goto Label_0073;
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    appFileVersion = FileVersionInfo.GetVersionInfo(ExecutablePath);
                }
            Label_0073:;
            }
            return (FileVersionInfo) appFileVersion;
        }

        private static System.Type GetAppMainType()
        {
            lock (internalSyncObject)
            {
                if (mainType == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null)
                    {
                        mainType = entryAssembly.EntryPoint.ReflectedType;
                    }
                }
            }
            return mainType;
        }

        private static ThreadContext GetContextForHandle(HandleRef handle)
        {
            int num;
            return ThreadContext.FromId(System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(handle, out num));
        }

        private static string GetDataPath(string basePath)
        {
            string format = @"{0}\{1}\{2}\{3}";
            string companyName = CompanyName;
            string productName = ProductName;
            string productVersion = ProductVersion;
            string path = string.Format(CultureInfo.CurrentCulture, format, new object[] { basePath, companyName, productName, productVersion });
            lock (internalSyncObject)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            return path;
        }

        public static ApartmentState OleRequired()
        {
            return ThreadContext.FromCurrent().OleRequired();
        }

        public static void OnThreadException(Exception t)
        {
            ThreadContext.FromCurrent().OnThreadException(t);
        }

        internal static void OpenFormsInternalAdd(Form form)
        {
            OpenFormsInternal.Add(form);
        }

        internal static void OpenFormsInternalRemove(Form form)
        {
            OpenFormsInternal.Remove(form);
        }

        internal static void ParkHandle(HandleRef handle)
        {
            ThreadContext contextForHandle = GetContextForHandle(handle);
            if (contextForHandle != null)
            {
                contextForHandle.ParkingWindow.ParkHandle(handle);
            }
        }

        internal static void ParkHandle(CreateParams cp)
        {
            ThreadContext context = ThreadContext.FromCurrent();
            if (context != null)
            {
                cp.Parent = context.ParkingWindow.Handle;
            }
        }

        private static void RaiseExit()
        {
            if (eventHandlers != null)
            {
                Delegate delegate2 = eventHandlers[EVENT_APPLICATIONEXIT];
                if (delegate2 != null)
                {
                    ((EventHandler) delegate2)(null, EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void RaiseIdle(EventArgs e)
        {
            ThreadContext context = ThreadContext.FromCurrent();
            if (context.idleHandler != null)
            {
                context.idleHandler(Thread.CurrentThread, e);
            }
        }

        private static void RaiseThreadExit()
        {
            if (eventHandlers != null)
            {
                Delegate delegate2 = eventHandlers[EVENT_THREADEXIT];
                if (delegate2 != null)
                {
                    ((EventHandler) delegate2)(null, EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void RegisterMessageLoop(MessageLoopCallback callback)
        {
            ThreadContext.FromCurrent().RegisterMessageLoop(callback);
        }

        private static void RemoveEventHandler(object key, Delegate value)
        {
            lock (internalSyncObject)
            {
                if (eventHandlers != null)
                {
                    eventHandlers.RemoveHandler(key, value);
                }
            }
        }

        public static void RemoveMessageFilter(IMessageFilter value)
        {
            ThreadContext.FromCurrent().RemoveMessageFilter(value);
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void Restart()
        {
            if (Assembly.GetEntryAssembly() == null)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("RestartNotSupported"));
            }
            bool flag = false;
            Process currentProcess = Process.GetCurrentProcess();
            if (string.Equals(currentProcess.MainModule.ModuleName, "ieexec.exe", StringComparison.OrdinalIgnoreCase))
            {
                string directoryName = string.Empty;
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    directoryName = Path.GetDirectoryName(typeof(object).Module.FullyQualifiedName);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (string.Equals(directoryName + @"\ieexec.exe", currentProcess.MainModule.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    ExitInternal();
                    string data = AppDomain.CurrentDomain.GetData("APP_LAUNCH_URL") as string;
                    if (data != null)
                    {
                        Process.Start(currentProcess.MainModule.FileName, data);
                    }
                }
            }
            if (!flag)
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    string updatedApplicationFullName = ApplicationDeployment.CurrentDeployment.UpdatedApplicationFullName;
                    uint hostTypeFromMetaData = (uint) ClickOnceUtility.GetHostTypeFromMetaData(updatedApplicationFullName);
                    ExitInternal();
                    System.Windows.Forms.UnsafeNativeMethods.CorLaunchApplication(hostTypeFromMetaData, updatedApplicationFullName, 0, null, 0, null, new System.Windows.Forms.UnsafeNativeMethods.PROCESS_INFORMATION());
                }
                else
                {
                    string[] commandLineArgs = Environment.GetCommandLineArgs();
                    StringBuilder builder = new StringBuilder((commandLineArgs.Length - 1) * 0x10);
                    for (int i = 1; i < (commandLineArgs.Length - 1); i++)
                    {
                        builder.Append('"');
                        builder.Append(commandLineArgs[i]);
                        builder.Append("\" ");
                    }
                    if (commandLineArgs.Length > 1)
                    {
                        builder.Append('"');
                        builder.Append(commandLineArgs[commandLineArgs.Length - 1]);
                        builder.Append('"');
                    }
                    ProcessStartInfo startInfo = Process.GetCurrentProcess().StartInfo;
                    startInfo.FileName = ExecutablePath;
                    if (builder.Length > 0)
                    {
                        startInfo.Arguments = builder.ToString();
                    }
                    ExitInternal();
                    Process.Start(startInfo);
                }
            }
        }

        public static void Run()
        {
            ThreadContext.FromCurrent().RunMessageLoop(-1, new System.Windows.Forms.ApplicationContext());
        }

        public static void Run(System.Windows.Forms.ApplicationContext context)
        {
            ThreadContext.FromCurrent().RunMessageLoop(-1, context);
        }

        public static void Run(Form mainForm)
        {
            ThreadContext.FromCurrent().RunMessageLoop(-1, new System.Windows.Forms.ApplicationContext(mainForm));
        }

        internal static void RunDialog(Form form)
        {
            ThreadContext.FromCurrent().RunMessageLoop(4, new ModalApplicationContext(form));
        }

        private static bool SendThemeChanged(IntPtr handle, IntPtr extraParameter)
        {
            int num;
            int currentProcessId = System.Windows.Forms.SafeNativeMethods.GetCurrentProcessId();
            System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, handle), out num);
            if ((num == currentProcessId) && System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(null, handle)))
            {
                SendThemeChangedRecursive(handle, IntPtr.Zero);
                System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(null, handle), (System.Windows.Forms.NativeMethods.COMRECT) null, System.Windows.Forms.NativeMethods.NullHandleRef, 0x485);
            }
            return true;
        }

        private static bool SendThemeChangedRecursive(IntPtr handle, IntPtr lparam)
        {
            System.Windows.Forms.UnsafeNativeMethods.EnumChildWindows(new HandleRef(null, handle), new System.Windows.Forms.NativeMethods.EnumChildrenCallback(Application.SendThemeChangedRecursive), System.Windows.Forms.NativeMethods.NullHandleRef);
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, handle), 0x31a, 0, 0);
            return true;
        }

        public static void SetCompatibleTextRenderingDefault(bool defaultValue)
        {
            if (NativeWindow.AnyHandleCreated)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("Win32WindowAlreadyCreated"));
            }
            Control.UseCompatibleTextRenderingDefault = defaultValue;
        }

        public static bool SetSuspendState(PowerState state, bool force, bool disableWakeEvent)
        {
            System.Windows.Forms.IntSecurity.AffectMachineState.Demand();
            return System.Windows.Forms.UnsafeNativeMethods.SetSuspendState(state == PowerState.Hibernate, force, disableWakeEvent);
        }

        public static void SetUnhandledExceptionMode(UnhandledExceptionMode mode)
        {
            SetUnhandledExceptionMode(mode, true);
        }

        public static void SetUnhandledExceptionMode(UnhandledExceptionMode mode, bool threadScope)
        {
            System.Windows.Forms.IntSecurity.AffectThreadBehavior.Demand();
            NativeWindow.SetUnhandledExceptionModeInternal(mode, threadScope);
        }

        internal static void UnparkHandle(HandleRef handle)
        {
            ThreadContext contextForHandle = GetContextForHandle(handle);
            if (contextForHandle != null)
            {
                contextForHandle.ParkingWindow.UnparkHandle(handle);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static void UnregisterMessageLoop()
        {
            ThreadContext.FromCurrent().RegisterMessageLoop(null);
        }

        public static bool AllowQuit
        {
            get
            {
                return ThreadContext.FromCurrent().GetAllowQuit();
            }
        }

        internal static bool CanContinueIdle
        {
            get
            {
                return ThreadContext.FromCurrent().ComponentManager.FContinueIdle();
            }
        }

        internal static bool ComCtlSupportsVisualStyles
        {
            get
            {
                if (useVisualStyles && OSFeature.Feature.IsPresent(OSFeature.Themes))
                {
                    return true;
                }
                IntPtr moduleHandle = System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle("comctl32.dll");
                if (moduleHandle != IntPtr.Zero)
                {
                    try
                    {
                        return (System.Windows.Forms.UnsafeNativeMethods.GetProcAddress(new HandleRef(null, moduleHandle), "ImageList_WriteEx") != IntPtr.Zero);
                    }
                    catch
                    {
                        goto Label_009B;
                    }
                }
                moduleHandle = System.Windows.Forms.UnsafeNativeMethods.LoadLibrary("comctl32.dll");
                if (moduleHandle != IntPtr.Zero)
                {
                    try
                    {
                        return (System.Windows.Forms.UnsafeNativeMethods.GetProcAddress(new HandleRef(null, moduleHandle), "ImageList_WriteEx") != IntPtr.Zero);
                    }
                    finally
                    {
                        System.Windows.Forms.UnsafeNativeMethods.FreeLibrary(new HandleRef(null, moduleHandle));
                    }
                }
            Label_009B:
                return false;
            }
        }

        public static string CommonAppDataPath
        {
            get
            {
                try
                {
                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            }
        }

        public static RegistryKey CommonAppDataRegistry
        {
            get
            {
                return Registry.LocalMachine.CreateSubKey(CommonAppDataRegistryKeyName);
            }
        }

        internal static string CommonAppDataRegistryKeyName
        {
            get
            {
                string format = @"Software\{0}\{1}\{2}";
                return string.Format(CultureInfo.CurrentCulture, format, new object[] { CompanyName, ProductName, ProductVersion });
            }
        }

        public static string CompanyName
        {
            get
            {
                lock (internalSyncObject)
                {
                    if (companyName == null)
                    {
                        Assembly entryAssembly = Assembly.GetEntryAssembly();
                        if (entryAssembly != null)
                        {
                            object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                            if ((customAttributes != null) && (customAttributes.Length > 0))
                            {
                                companyName = ((AssemblyCompanyAttribute) customAttributes[0]).Company;
                            }
                        }
                        if ((companyName == null) || (companyName.Length == 0))
                        {
                            companyName = GetAppFileVersionInfo().CompanyName;
                            if (companyName != null)
                            {
                                companyName = companyName.Trim();
                            }
                        }
                        if ((companyName == null) || (companyName.Length == 0))
                        {
                            System.Type appMainType = GetAppMainType();
                            if (appMainType != null)
                            {
                                string str = appMainType.Namespace;
                                if (!string.IsNullOrEmpty(str))
                                {
                                    int index = str.IndexOf(".");
                                    if (index != -1)
                                    {
                                        companyName = str.Substring(0, index);
                                    }
                                    else
                                    {
                                        companyName = str;
                                    }
                                }
                                else
                                {
                                    companyName = ProductName;
                                }
                            }
                        }
                    }
                }
                return companyName;
            }
        }

        public static CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
            set
            {
                Thread.CurrentThread.CurrentCulture = value;
            }
        }

        public static InputLanguage CurrentInputLanguage
        {
            get
            {
                return InputLanguage.CurrentInputLanguage;
            }
            set
            {
                System.Windows.Forms.IntSecurity.AffectThreadBehavior.Demand();
                InputLanguage.CurrentInputLanguage = value;
            }
        }

        internal static bool CustomThreadExceptionHandlerAttached
        {
            get
            {
                return ThreadContext.FromCurrent().CustomThreadExceptionHandlerAttached;
            }
        }

        public static string ExecutablePath
        {
            get
            {
                if (executablePath == null)
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        StringBuilder buffer = new StringBuilder(260);
                        System.Windows.Forms.UnsafeNativeMethods.GetModuleFileName(System.Windows.Forms.NativeMethods.NullHandleRef, buffer, buffer.Capacity);
                        executablePath = System.Windows.Forms.IntSecurity.UnsafeGetFullPath(buffer.ToString());
                    }
                    else
                    {
                        string escapedCodeBase = entryAssembly.EscapedCodeBase;
                        Uri uri = new Uri(escapedCodeBase);
                        if (uri.Scheme == "file")
                        {
                            executablePath = System.Windows.Forms.NativeMethods.GetLocalPath(escapedCodeBase);
                        }
                        else
                        {
                            executablePath = uri.ToString();
                        }
                    }
                }
                Uri uri2 = new Uri(executablePath);
                if (uri2.Scheme == "file")
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, executablePath).Demand();
                }
                return executablePath;
            }
        }

        public static string LocalUserAppDataPath
        {
            get
            {
                try
                {
                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            }
        }

        public static bool MessageLoop
        {
            get
            {
                return ThreadContext.FromCurrent().GetMessageLoop();
            }
        }

        public static FormCollection OpenForms
        {
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            get
            {
                return OpenFormsInternal;
            }
        }

        internal static FormCollection OpenFormsInternal
        {
            get
            {
                if (forms == null)
                {
                    forms = new FormCollection();
                }
                return forms;
            }
        }

        public static string ProductName
        {
            get
            {
                lock (internalSyncObject)
                {
                    if (productName == null)
                    {
                        Assembly entryAssembly = Assembly.GetEntryAssembly();
                        if (entryAssembly != null)
                        {
                            object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                            if ((customAttributes != null) && (customAttributes.Length > 0))
                            {
                                productName = ((AssemblyProductAttribute) customAttributes[0]).Product;
                            }
                        }
                        if ((productName == null) || (productName.Length == 0))
                        {
                            productName = GetAppFileVersionInfo().ProductName;
                            if (productName != null)
                            {
                                productName = productName.Trim();
                            }
                        }
                        if ((productName == null) || (productName.Length == 0))
                        {
                            System.Type appMainType = GetAppMainType();
                            if (appMainType != null)
                            {
                                string str = appMainType.Namespace;
                                if (!string.IsNullOrEmpty(str))
                                {
                                    int num = str.LastIndexOf(".");
                                    if ((num != -1) && (num < (str.Length - 1)))
                                    {
                                        productName = str.Substring(num + 1);
                                    }
                                    else
                                    {
                                        productName = str;
                                    }
                                }
                                else
                                {
                                    productName = appMainType.Name;
                                }
                            }
                        }
                    }
                }
                return productName;
            }
        }

        public static string ProductVersion
        {
            get
            {
                lock (internalSyncObject)
                {
                    if (productVersion == null)
                    {
                        Assembly entryAssembly = Assembly.GetEntryAssembly();
                        if (entryAssembly != null)
                        {
                            object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                            if ((customAttributes != null) && (customAttributes.Length > 0))
                            {
                                productVersion = ((AssemblyInformationalVersionAttribute) customAttributes[0]).InformationalVersion;
                            }
                        }
                        if ((productVersion == null) || (productVersion.Length == 0))
                        {
                            productVersion = GetAppFileVersionInfo().ProductVersion;
                            if (productVersion != null)
                            {
                                productVersion = productVersion.Trim();
                            }
                        }
                        if ((productVersion == null) || (productVersion.Length == 0))
                        {
                            productVersion = "1.0.0.0";
                        }
                    }
                }
                return productVersion;
            }
        }

        public static bool RenderWithVisualStyles
        {
            get
            {
                return (ComCtlSupportsVisualStyles && VisualStyleRenderer.IsSupported);
            }
        }

        public static string SafeTopLevelCaptionFormat
        {
            get
            {
                if (safeTopLevelCaptionSuffix == null)
                {
                    safeTopLevelCaptionSuffix = System.Windows.Forms.SR.GetString("SafeTopLevelCaptionFormat");
                }
                return safeTopLevelCaptionSuffix;
            }
            set
            {
                System.Windows.Forms.IntSecurity.WindowAdornmentModification.Demand();
                if (value == null)
                {
                    value = string.Empty;
                }
                safeTopLevelCaptionSuffix = value;
            }
        }

        public static string StartupPath
        {
            get
            {
                if (startupPath == null)
                {
                    StringBuilder buffer = new StringBuilder(260);
                    System.Windows.Forms.UnsafeNativeMethods.GetModuleFileName(System.Windows.Forms.NativeMethods.NullHandleRef, buffer, buffer.Capacity);
                    startupPath = Path.GetDirectoryName(buffer.ToString());
                }
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, startupPath).Demand();
                return startupPath;
            }
        }

        internal static bool UseEverettThreadAffinity
        {
            get
            {
                if (!checkedThreadAffinity)
                {
                    checkedThreadAffinity = true;
                    try
                    {
                        new RegistryPermission(PermissionState.Unrestricted).Assert();
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(CommonAppDataRegistryKeyName);
                        if (key != null)
                        {
                            object obj2 = key.GetValue("EnableSystemEventsThreadAffinityCompatibility");
                            key.Close();
                            if ((obj2 != null) && (((int) obj2) != 0))
                            {
                                useEverettThreadAffinity = true;
                            }
                        }
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (InvalidCastException)
                    {
                    }
                }
                return useEverettThreadAffinity;
            }
        }

        public static string UserAppDataPath
        {
            get
            {
                try
                {
                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            }
        }

        public static RegistryKey UserAppDataRegistry
        {
            get
            {
                string format = @"Software\{0}\{1}\{2}";
                return Registry.CurrentUser.CreateSubKey(string.Format(CultureInfo.CurrentCulture, format, new object[] { CompanyName, ProductName, ProductVersion }));
            }
        }

        internal static bool UseVisualStyles
        {
            get
            {
                return useVisualStyles;
            }
        }

        public static bool UseWaitCursor
        {
            get
            {
                return useWaitCursor;
            }
            set
            {
                lock (FormCollection.CollectionSyncRoot)
                {
                    useWaitCursor = value;
                    foreach (Form form in OpenFormsInternal)
                    {
                        form.UseWaitCursor = useWaitCursor;
                    }
                }
            }
        }

        public static System.Windows.Forms.VisualStyles.VisualStyleState VisualStyleState
        {
            get
            {
                if (!VisualStyleInformation.IsSupportedByOS)
                {
                    return System.Windows.Forms.VisualStyles.VisualStyleState.NoneEnabled;
                }
                return (System.Windows.Forms.VisualStyles.VisualStyleState) System.Windows.Forms.SafeNativeMethods.GetThemeAppProperties();
            }
            set
            {
                if (VisualStyleInformation.IsSupportedByOS)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.VisualStyles.VisualStyleState));
                    }
                    System.Windows.Forms.SafeNativeMethods.SetThemeAppProperties((int) value);
                    System.Windows.Forms.SafeNativeMethods.EnumThreadWindowsCallback callback = new System.Windows.Forms.SafeNativeMethods.EnumThreadWindowsCallback(Application.SendThemeChanged);
                    System.Windows.Forms.SafeNativeMethods.EnumWindows(callback, IntPtr.Zero);
                    GC.KeepAlive(callback);
                }
            }
        }

        internal static string WindowMessagesVersion
        {
            get
            {
                return "WindowsForms12";
            }
        }

        internal static string WindowsFormsVersion
        {
            get
            {
                return "WindowsForms10";
            }
        }

        private class ClickOnceUtility
        {
            private ClickOnceUtility()
            {
            }

            public static HostType GetHostTypeFromMetaData(string appFullName)
            {
                HostType type = HostType.Default;
                try
                {
                    type = GetPropertyBoolean(System.Deployment.Internal.Isolation.IsolationInterop.AppIdAuthority.TextToDefinition(0, appFullName), "IsFullTrust") ? HostType.CorFlag : HostType.AppLaunch;
                }
                catch
                {
                }
                return type;
            }

            private static bool GetPropertyBoolean(System.Deployment.Internal.Isolation.IDefinitionAppId appId, string propName)
            {
                string propertyString = GetPropertyString(appId, propName);
                if (string.IsNullOrEmpty(propertyString))
                {
                    return false;
                }
                try
                {
                    return Convert.ToBoolean(propertyString, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return false;
                }
            }

            private static string GetPropertyString(System.Deployment.Internal.Isolation.IDefinitionAppId appId, string propName)
            {
                byte[] bytes = System.Deployment.Internal.Isolation.IsolationInterop.UserStore.GetDeploymentProperty(System.Deployment.Internal.Isolation.Store.GetPackagePropertyFlags.Nothing, appId, InstallReference, new Guid("2ad613da-6fdb-4671-af9e-18ab2e4df4d8"), propName);
                int length = bytes.Length;
                if (((length != 0) && ((bytes.Length % 2) == 0)) && ((bytes[length - 2] == 0) && (bytes[length - 1] == 0)))
                {
                    return Encoding.Unicode.GetString(bytes, 0, length - 2);
                }
                return null;
            }

            private static System.Deployment.Internal.Isolation.StoreApplicationReference InstallReference
            {
                get
                {
                    return new System.Deployment.Internal.Isolation.StoreApplicationReference(System.Deployment.Internal.Isolation.IsolationInterop.GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING, "{3f471841-eef2-47d6-89c0-d028f03a4ad5}", null);
                }
            }

            public enum HostType
            {
                Default,
                AppLaunch,
                CorFlag
            }
        }

        private class ComponentManager : System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager
        {
            private System.Windows.Forms.UnsafeNativeMethods.IMsoComponent activeComponent;
            private int cookieCounter;
            private int currentState;
            private Hashtable oleComponents;
            private System.Windows.Forms.UnsafeNativeMethods.IMsoComponent trackingComponent;

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FContinueIdle()
            {
                System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
                return !System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0);
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FCreateSubComponentManager(object punkOuter, object punkServProv, ref Guid riid, out IntPtr ppvObj)
            {
                ppvObj = IntPtr.Zero;
                return false;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FDebugMessage(IntPtr hInst, int msg, IntPtr wparam, IntPtr lparam)
            {
                return true;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FGetActiveComponent(int dwgac, System.Windows.Forms.UnsafeNativeMethods.IMsoComponent[] ppic, System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT info, int dwReserved)
            {
                System.Windows.Forms.UnsafeNativeMethods.IMsoComponent activeComponent = null;
                if (dwgac == 0)
                {
                    activeComponent = this.activeComponent;
                }
                else if (dwgac == 1)
                {
                    activeComponent = this.trackingComponent;
                }
                else if (dwgac == 2)
                {
                    if (this.trackingComponent != null)
                    {
                        activeComponent = this.trackingComponent;
                    }
                    else
                    {
                        activeComponent = this.activeComponent;
                    }
                }
                if (ppic != null)
                {
                    ppic[0] = activeComponent;
                }
                if ((info != null) && (activeComponent != null))
                {
                    foreach (ComponentHashtableEntry entry in this.OleComponents.Values)
                    {
                        if (entry.component == activeComponent)
                        {
                            info = entry.componentInfo;
                            break;
                        }
                    }
                }
                return (activeComponent != null);
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FGetParentComponentManager(out System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager ppicm)
            {
                ppicm = null;
                return false;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FInState(int uStateID, IntPtr pvoid)
            {
                return ((this.currentState & uStateID) != 0);
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FOnComponentActivate(IntPtr dwComponentID)
            {
                int num = (int) ((long) dwComponentID);
                ComponentHashtableEntry entry = (ComponentHashtableEntry) this.OleComponents[num];
                if (entry == null)
                {
                    return false;
                }
                this.activeComponent = entry.component;
                return true;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FOnComponentExitState(IntPtr dwComponentID, int uStateID, int uContext, int cpicmExclude, int rgpicmExclude)
            {
                long num1 = (long) dwComponentID;
                this.currentState &= ~uStateID;
                if ((uContext == 0) || (uContext == 1))
                {
                    foreach (ComponentHashtableEntry entry in this.OleComponents.Values)
                    {
                        entry.component.OnEnterState(uStateID, false);
                    }
                }
                return false;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, int reason, int pvLoopData)
            {
                int key = (int) ((long) dwComponentID);
                int currentState = this.currentState;
                bool flag = true;
                if (!this.OleComponents.ContainsKey(key))
                {
                    return false;
                }
                System.Windows.Forms.UnsafeNativeMethods.IMsoComponent activeComponent = this.activeComponent;
                try
                {
                    System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
                    System.Windows.Forms.NativeMethods.MSG[] pMsgPeeked = new System.Windows.Forms.NativeMethods.MSG[] { msg };
                    bool flag2 = false;
                    ComponentHashtableEntry entry = (ComponentHashtableEntry) this.OleComponents[key];
                    if (entry == null)
                    {
                        return false;
                    }
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponent component = entry.component;
                    this.activeComponent = component;
                    while (flag)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.IMsoComponent trackingComponent;
                        if (this.trackingComponent != null)
                        {
                            trackingComponent = this.trackingComponent;
                        }
                        else if (this.activeComponent != null)
                        {
                            trackingComponent = this.activeComponent;
                        }
                        else
                        {
                            trackingComponent = component;
                        }
                        if (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0))
                        {
                            pMsgPeeked[0] = msg;
                            flag = trackingComponent.FContinueMessageLoop(reason, pvLoopData, pMsgPeeked);
                            if (flag)
                            {
                                if ((msg.hwnd != IntPtr.Zero) && System.Windows.Forms.SafeNativeMethods.IsWindowUnicode(new HandleRef(null, msg.hwnd)))
                                {
                                    flag2 = true;
                                    System.Windows.Forms.UnsafeNativeMethods.GetMessageW(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0);
                                }
                                else
                                {
                                    flag2 = false;
                                    System.Windows.Forms.UnsafeNativeMethods.GetMessageA(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0);
                                }
                                if (msg.message == 0x12)
                                {
                                    Application.ThreadContext.FromCurrent().DisposeThreadWindows();
                                    if (reason != -1)
                                    {
                                        System.Windows.Forms.UnsafeNativeMethods.PostQuitMessage((int) msg.wParam);
                                    }
                                    flag = false;
                                    goto Label_024C;
                                }
                                if (!trackingComponent.FPreTranslateMessage(ref msg))
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.TranslateMessage(ref msg);
                                    if (flag2)
                                    {
                                        System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(ref msg);
                                    }
                                    else
                                    {
                                        System.Windows.Forms.UnsafeNativeMethods.DispatchMessageA(ref msg);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((reason == 2) || (reason == -2))
                            {
                                goto Label_024C;
                            }
                            bool flag4 = false;
                            if (this.OleComponents != null)
                            {
                                IEnumerator enumerator = this.OleComponents.Values.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    ComponentHashtableEntry current = (ComponentHashtableEntry) enumerator.Current;
                                    flag4 |= current.component.FDoIdle(-1);
                                }
                            }
                            flag = trackingComponent.FContinueMessageLoop(reason, pvLoopData, null);
                            if (flag)
                            {
                                if (flag4)
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.MsgWaitForMultipleObjectsEx(0, IntPtr.Zero, 100, 0xff, 4);
                                }
                                else if (!System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0))
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.WaitMessage();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.currentState = currentState;
                    this.activeComponent = activeComponent;
                }
            Label_024C:
                return !flag;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FRegisterComponent(System.Windows.Forms.UnsafeNativeMethods.IMsoComponent component, System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT pcrinfo, out IntPtr dwComponentID)
            {
                ComponentHashtableEntry entry = new ComponentHashtableEntry {
                    component = component,
                    componentInfo = pcrinfo
                };
                this.OleComponents.Add(++this.cookieCounter, entry);
                dwComponentID = (IntPtr) this.cookieCounter;
                return true;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FRevokeComponent(IntPtr dwComponentID)
            {
                int key = (int) ((long) dwComponentID);
                ComponentHashtableEntry entry = (ComponentHashtableEntry) this.OleComponents[key];
                if (entry == null)
                {
                    return false;
                }
                if (entry.component == this.activeComponent)
                {
                    this.activeComponent = null;
                }
                if (entry.component == this.trackingComponent)
                {
                    this.trackingComponent = null;
                }
                this.OleComponents.Remove(key);
                return true;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FSetTrackingComponent(IntPtr dwComponentID, bool fTrack)
            {
                int num = (int) ((long) dwComponentID);
                ComponentHashtableEntry entry = (ComponentHashtableEntry) this.OleComponents[num];
                if (entry == null)
                {
                    return false;
                }
                if ((entry.component == this.trackingComponent) ^ fTrack)
                {
                    return false;
                }
                if (fTrack)
                {
                    this.trackingComponent = entry.component;
                }
                else
                {
                    this.trackingComponent = null;
                }
                return true;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FUpdateComponentRegistration(IntPtr dwComponentID, System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT info)
            {
                int num = (int) ((long) dwComponentID);
                ComponentHashtableEntry entry = (ComponentHashtableEntry) this.OleComponents[num];
                if (entry == null)
                {
                    return false;
                }
                entry.componentInfo = info;
                return true;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.OnComponentEnterState(IntPtr dwComponentID, int uStateID, int uContext, int cpicmExclude, int rgpicmExclude, int dwReserved)
            {
                long num1 = (long) dwComponentID;
                this.currentState |= uStateID;
                if ((uContext == 0) || (uContext == 1))
                {
                    foreach (ComponentHashtableEntry entry in this.OleComponents.Values)
                    {
                        entry.component.OnEnterState(uStateID, true);
                    }
                }
            }

            int System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.QueryService(ref Guid guidService, ref Guid iid, out object ppvObj)
            {
                ppvObj = null;
                return -2147467262;
            }

            private Hashtable OleComponents
            {
                get
                {
                    if (this.oleComponents == null)
                    {
                        this.oleComponents = new Hashtable();
                        this.cookieCounter = 0;
                    }
                    return this.oleComponents;
                }
            }

            private class ComponentHashtableEntry
            {
                public System.Windows.Forms.UnsafeNativeMethods.IMsoComponent component;
                public System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT componentInfo;
            }
        }

        internal sealed class MarshalingControl : Control
        {
            internal MarshalingControl() : base(false)
            {
                base.Visible = false;
                base.SetState2(8, false);
                base.SetTopLevel(true);
                base.CreateControl();
                this.CreateHandle();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
            }

            protected override void OnSizeChanged(EventArgs e)
            {
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        createParams.Parent = (IntPtr) System.Windows.Forms.NativeMethods.HWND_MESSAGE;
                    }
                    return createParams;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public delegate bool MessageLoopCallback();

        private class ModalApplicationContext : System.Windows.Forms.ApplicationContext
        {
            private Application.ThreadContext parentWindowContext;

            public ModalApplicationContext(Form modalForm) : base(modalForm)
            {
            }

            public void DisableThreadWindows(bool disable, bool onlyWinForms)
            {
                Control mainForm = null;
                if ((base.MainForm != null) && base.MainForm.IsHandleCreated)
                {
                    IntPtr windowLong = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.MainForm.Handle), -8);
                    mainForm = Control.FromHandleInternal(windowLong);
                    if ((mainForm != null) && mainForm.InvokeRequired)
                    {
                        this.parentWindowContext = Application.GetContextForHandle(new HandleRef(this, windowLong));
                    }
                    else
                    {
                        this.parentWindowContext = null;
                    }
                }
                if (this.parentWindowContext != null)
                {
                    if (mainForm == null)
                    {
                        mainForm = this.parentWindowContext.ApplicationContext.MainForm;
                    }
                    if (disable)
                    {
                        mainForm.Invoke(new ThreadWindowCallback(this.DisableThreadWindowsCallback), new object[] { this.parentWindowContext, onlyWinForms });
                    }
                    else
                    {
                        mainForm.Invoke(new ThreadWindowCallback(this.EnableThreadWindowsCallback), new object[] { this.parentWindowContext, onlyWinForms });
                    }
                }
            }

            private void DisableThreadWindowsCallback(Application.ThreadContext context, bool onlyWinForms)
            {
                context.DisableWindowsForModalLoop(onlyWinForms, this);
            }

            private void EnableThreadWindowsCallback(Application.ThreadContext context, bool onlyWinForms)
            {
                context.EnableWindowsForModalLoop(onlyWinForms, this);
            }

            protected override void ExitThreadCore()
            {
            }

            private delegate void ThreadWindowCallback(Application.ThreadContext context, bool onlyWinForms);
        }

        internal sealed class ParkingWindow : ContainerControl, IArrangedElement, IComponent, IDisposable
        {
            private int childCount;
            private const int WM_CHECKDESTROY = 0x401;

            public ParkingWindow()
            {
                base.SetState2(8, false);
                base.SetState(0x80000, true);
                this.Text = "WindowsFormsParkingWindow";
                base.Visible = false;
            }

            internal override void AddReflectChild()
            {
                if (this.childCount < 0)
                {
                    this.childCount = 0;
                }
                this.childCount++;
            }

            private void CheckDestroy()
            {
                if ((this.childCount == 0) && (System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(this, base.Handle), 5) == IntPtr.Zero))
                {
                    this.DestroyHandle();
                }
            }

            public void Destroy()
            {
                this.DestroyHandle();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
            }

            internal void ParkHandle(HandleRef handle)
            {
                if (!base.IsHandleCreated)
                {
                    this.CreateHandle();
                }
                System.Windows.Forms.UnsafeNativeMethods.SetParent(handle, new HandleRef(this, base.Handle));
            }

            internal override void RemoveReflectChild()
            {
                this.childCount--;
                if (this.childCount < 0)
                {
                    this.childCount = 0;
                }
                if ((this.childCount == 0) && base.IsHandleCreated)
                {
                    int num;
                    Application.ThreadContext objA = Application.ThreadContext.FromId(System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this, base.HandleInternal), out num));
                    if ((objA == null) || !object.ReferenceEquals(objA, Application.ThreadContext.FromCurrent()))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.HandleInternal), 0x401, IntPtr.Zero, IntPtr.Zero);
                    }
                    else
                    {
                        this.CheckDestroy();
                    }
                }
            }

            void IArrangedElement.PerformLayout(IArrangedElement affectedElement, string affectedProperty)
            {
            }

            internal void UnparkHandle(HandleRef handle)
            {
                if (base.IsHandleCreated)
                {
                    this.CheckDestroy();
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg != 0x18)
                {
                    base.WndProc(ref m);
                    if (m.Msg == 0x210)
                    {
                        if (System.Windows.Forms.NativeMethods.Util.LOWORD((int) ((long) m.WParam)) == 2)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x401, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    else if (m.Msg == 0x401)
                    {
                        this.CheckDestroy();
                    }
                }
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        createParams.Parent = (IntPtr) System.Windows.Forms.NativeMethods.HWND_MESSAGE;
                    }
                    return createParams;
                }
            }
        }

        internal sealed class ThreadContext : MarshalByRefObject, System.Windows.Forms.UnsafeNativeMethods.IMsoComponent
        {
            private WeakReference activatingControlRef;
            private System.Windows.Forms.ApplicationContext applicationContext;
            private static int baseLoopReason;
            private int componentID = -1;
            private System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager;
            private static Hashtable contextHash = new Hashtable();
            private CultureInfo culture;
            private Form currentForm;
            [ThreadStatic]
            private static Application.ThreadContext currentThreadContext;
            private int disposeCount;
            internal EventHandler enterModalHandler;
            private bool externalComponentManager;
            private bool fetchingComponentManager;
            private IntPtr handle;
            private int id;
            internal EventHandler idleHandler;
            private const int INVALID_ID = -1;
            internal EventHandler leaveModalHandler;
            private Control marshalingControl;
            private ArrayList messageFilters;
            private IMessageFilter[] messageFilterSnapshot;
            private Application.MessageLoopCallback messageLoopCallback;
            private int messageLoopCount;
            private int modalCount;
            private bool ourModalLoop;
            private System.Windows.Forms.Application.ParkingWindow parkingWindow;
            private const int STATE_EXTERNALOLEINIT = 2;
            private const int STATE_FILTERSNAPSHOTVALID = 0x10;
            private const int STATE_INTHREADEXCEPTION = 4;
            private const int STATE_OLEINITIALIZED = 1;
            private const int STATE_POSTEDQUIT = 8;
            private const int STATE_TRACKINGCOMPONENT = 0x20;
            private static object tcInternalSyncObject = new object();
            private System.Windows.Forms.NativeMethods.MSG tempMsg = new System.Windows.Forms.NativeMethods.MSG();
            internal ThreadExceptionEventHandler threadExceptionHandler;
            private int threadState;
            private Application.ThreadWindows threadWindows;
            private static int totalMessageLoopCount;

            public ThreadContext()
            {
                IntPtr zero = IntPtr.Zero;
                System.Windows.Forms.UnsafeNativeMethods.DuplicateHandle(new HandleRef(null, System.Windows.Forms.SafeNativeMethods.GetCurrentProcess()), new HandleRef(null, System.Windows.Forms.SafeNativeMethods.GetCurrentThread()), new HandleRef(null, System.Windows.Forms.SafeNativeMethods.GetCurrentProcess()), ref zero, 0, false, 2);
                this.handle = zero;
                this.id = System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId();
                this.messageLoopCount = 0;
                currentThreadContext = this;
                contextHash[this.id] = this;
            }

            internal void AddMessageFilter(IMessageFilter f)
            {
                if (this.messageFilters == null)
                {
                    this.messageFilters = new ArrayList();
                }
                if (f != null)
                {
                    this.SetState(0x10, false);
                    if ((this.messageFilters.Count > 0) && (f is IMessageModifyAndFilter))
                    {
                        this.messageFilters.Insert(0, f);
                    }
                    else
                    {
                        this.messageFilters.Add(f);
                    }
                }
            }

            internal void BeginModalMessageLoop(System.Windows.Forms.ApplicationContext context)
            {
                bool ourModalLoop = this.ourModalLoop;
                this.ourModalLoop = true;
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = this.ComponentManager;
                    if (componentManager != null)
                    {
                        componentManager.OnComponentEnterState((IntPtr) this.componentID, 1, 0, 0, 0, 0);
                    }
                }
                finally
                {
                    this.ourModalLoop = ourModalLoop;
                }
                this.DisableWindowsForModalLoop(false, context);
                this.modalCount++;
                if ((this.enterModalHandler != null) && (this.modalCount == 1))
                {
                    this.enterModalHandler(Thread.CurrentThread, EventArgs.Empty);
                }
            }

            internal void DisableWindowsForModalLoop(bool onlyWinForms, System.Windows.Forms.ApplicationContext context)
            {
                Application.ThreadWindows threadWindows = this.threadWindows;
                this.threadWindows = new Application.ThreadWindows(onlyWinForms);
                this.threadWindows.Enable(false);
                this.threadWindows.previousThreadWindows = threadWindows;
                Application.ModalApplicationContext context2 = context as Application.ModalApplicationContext;
                if (context2 != null)
                {
                    context2.DisableThreadWindows(true, onlyWinForms);
                }
            }

            internal void Dispose(bool postQuit)
            {
                lock (this)
                {
                    try
                    {
                        if (this.disposeCount++ == 0)
                        {
                            if ((this.messageLoopCount > 0) && postQuit)
                            {
                                this.PostQuit();
                            }
                            else
                            {
                                bool flag = System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId() == this.id;
                                try
                                {
                                    if (flag)
                                    {
                                        if (this.componentManager != null)
                                        {
                                            this.RevokeComponent();
                                        }
                                        this.DisposeThreadWindows();
                                        try
                                        {
                                            Application.RaiseThreadExit();
                                        }
                                        finally
                                        {
                                            if (this.GetState(1) && !this.GetState(2))
                                            {
                                                this.SetState(1, false);
                                                System.Windows.Forms.UnsafeNativeMethods.OleUninitialize();
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    if (this.handle != IntPtr.Zero)
                                    {
                                        System.Windows.Forms.UnsafeNativeMethods.CloseHandle(new HandleRef(this, this.handle));
                                        this.handle = IntPtr.Zero;
                                    }
                                    try
                                    {
                                        if (totalMessageLoopCount == 0)
                                        {
                                            Application.RaiseExit();
                                        }
                                    }
                                    finally
                                    {
                                        contextHash.Remove(this.id);
                                        if (currentThreadContext == this)
                                        {
                                            currentThreadContext = null;
                                        }
                                    }
                                }
                            }
                            GC.SuppressFinalize(this);
                        }
                    }
                    finally
                    {
                        this.disposeCount--;
                    }
                }
            }

            private void DisposeParkingWindow()
            {
                if ((this.parkingWindow != null) && this.parkingWindow.IsHandleCreated)
                {
                    int num;
                    int windowThreadProcessId = System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this.parkingWindow, this.parkingWindow.Handle), out num);
                    int currentThreadId = System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId();
                    if (windowThreadProcessId == currentThreadId)
                    {
                        this.parkingWindow.Destroy();
                    }
                    else
                    {
                        this.parkingWindow = null;
                    }
                }
            }

            internal void DisposeThreadWindows()
            {
                try
                {
                    if (this.applicationContext != null)
                    {
                        this.applicationContext.Dispose();
                        this.applicationContext = null;
                    }
                    new Application.ThreadWindows(true).Dispose();
                    this.DisposeParkingWindow();
                }
                catch
                {
                }
            }

            internal void EnableWindowsForModalLoop(bool onlyWinForms, System.Windows.Forms.ApplicationContext context)
            {
                if (this.threadWindows != null)
                {
                    this.threadWindows.Enable(true);
                    this.threadWindows = this.threadWindows.previousThreadWindows;
                }
                Application.ModalApplicationContext context2 = context as Application.ModalApplicationContext;
                if (context2 != null)
                {
                    context2.DisableThreadWindows(false, onlyWinForms);
                }
            }

            internal void EndModalMessageLoop(System.Windows.Forms.ApplicationContext context)
            {
                this.EnableWindowsForModalLoop(false, context);
                bool ourModalLoop = this.ourModalLoop;
                this.ourModalLoop = true;
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = this.ComponentManager;
                    if (componentManager != null)
                    {
                        componentManager.FOnComponentExitState((IntPtr) this.componentID, 1, 0, 0, 0);
                    }
                }
                finally
                {
                    this.ourModalLoop = ourModalLoop;
                }
                this.modalCount--;
                if ((this.leaveModalHandler != null) && (this.modalCount == 0))
                {
                    this.leaveModalHandler(Thread.CurrentThread, EventArgs.Empty);
                }
            }

            internal static void ExitApplication()
            {
                ExitCommon(true);
            }

            private static void ExitCommon(bool disposing)
            {
                lock (tcInternalSyncObject)
                {
                    if (contextHash != null)
                    {
                        Application.ThreadContext[] array = new Application.ThreadContext[contextHash.Values.Count];
                        contextHash.Values.CopyTo(array, 0);
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i].ApplicationContext != null)
                            {
                                array[i].ApplicationContext.ExitThread();
                            }
                            else
                            {
                                array[i].Dispose(disposing);
                            }
                        }
                    }
                }
            }

            internal static void ExitDomain()
            {
                ExitCommon(false);
            }

            ~ThreadContext()
            {
                if (this.handle != IntPtr.Zero)
                {
                    System.Windows.Forms.UnsafeNativeMethods.CloseHandle(new HandleRef(this, this.handle));
                    this.handle = IntPtr.Zero;
                }
            }

            internal void FormActivated(bool activate)
            {
                if (activate)
                {
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = this.ComponentManager;
                    if ((componentManager != null) && !(componentManager is System.Windows.Forms.Application.ComponentManager))
                    {
                        componentManager.FOnComponentActivate((IntPtr) this.componentID);
                    }
                }
            }

            internal static Application.ThreadContext FromCurrent()
            {
                Application.ThreadContext currentThreadContext = Application.ThreadContext.currentThreadContext;
                if (currentThreadContext == null)
                {
                    currentThreadContext = new Application.ThreadContext();
                }
                return currentThreadContext;
            }

            internal static Application.ThreadContext FromId(int id)
            {
                Application.ThreadContext context = (Application.ThreadContext) contextHash[id];
                if ((context == null) && (id == System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId()))
                {
                    context = new Application.ThreadContext();
                }
                return context;
            }

            internal bool GetAllowQuit()
            {
                return ((totalMessageLoopCount > 0) && (baseLoopReason == -1));
            }

            internal CultureInfo GetCulture()
            {
                if ((this.culture == null) || (this.culture.LCID != System.Windows.Forms.SafeNativeMethods.GetThreadLocale()))
                {
                    this.culture = new CultureInfo(System.Windows.Forms.SafeNativeMethods.GetThreadLocale());
                }
                return this.culture;
            }

            internal IntPtr GetHandle()
            {
                return this.handle;
            }

            internal int GetId()
            {
                return this.id;
            }

            internal bool GetMessageLoop()
            {
                return this.GetMessageLoop(false);
            }

            internal bool GetMessageLoop(bool mustBeActive)
            {
                if (this.messageLoopCount > ((mustBeActive && this.externalComponentManager) ? 1 : 0))
                {
                    return true;
                }
                if ((this.ComponentManager != null) && this.externalComponentManager)
                {
                    if (!mustBeActive)
                    {
                        return true;
                    }
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponent[] ppic = new System.Windows.Forms.UnsafeNativeMethods.IMsoComponent[1];
                    if (this.ComponentManager.FGetActiveComponent(0, ppic, null, 0) && (ppic[0] == this))
                    {
                        return true;
                    }
                }
                Application.MessageLoopCallback messageLoopCallback = this.messageLoopCallback;
                return ((messageLoopCallback != null) && messageLoopCallback());
            }

            private bool GetState(int bit)
            {
                return ((this.threadState & bit) != 0);
            }

            public override object InitializeLifetimeService()
            {
                return null;
            }

            internal bool IsValidComponentId()
            {
                return (this.componentID != -1);
            }

            private bool LocalModalMessageLoop(Form form)
            {
                try
                {
                    System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
                    bool flag = false;
                    bool flag2 = true;
                    while (flag2)
                    {
                        if (!System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0))
                        {
                            goto Label_00AB;
                        }
                        if ((msg.hwnd != IntPtr.Zero) && System.Windows.Forms.SafeNativeMethods.IsWindowUnicode(new HandleRef(null, msg.hwnd)))
                        {
                            flag = true;
                            if (System.Windows.Forms.UnsafeNativeMethods.GetMessageW(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0))
                            {
                                goto Label_0074;
                            }
                            continue;
                        }
                        flag = false;
                        if (!System.Windows.Forms.UnsafeNativeMethods.GetMessageA(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0))
                        {
                            continue;
                        }
                    Label_0074:
                        if (!this.PreTranslateMessage(ref msg))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.TranslateMessage(ref msg);
                            if (flag)
                            {
                                System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(ref msg);
                            }
                            else
                            {
                                System.Windows.Forms.UnsafeNativeMethods.DispatchMessageA(ref msg);
                            }
                        }
                        if (form != null)
                        {
                            flag2 = !form.CheckCloseDialog(false);
                        }
                        continue;
                    Label_00AB:
                        if (form == null)
                        {
                            break;
                        }
                        if (!System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.WaitMessage();
                        }
                    }
                    return flag2;
                }
                catch
                {
                    return false;
                }
            }

            internal ApartmentState OleRequired()
            {
                Thread currentThread = Thread.CurrentThread;
                if (!this.GetState(1))
                {
                    int num = System.Windows.Forms.UnsafeNativeMethods.OleInitialize();
                    this.SetState(1, true);
                    if (num == -2147417850)
                    {
                        this.SetState(2, true);
                    }
                }
                if (this.GetState(2))
                {
                    return ApartmentState.MTA;
                }
                return ApartmentState.STA;
            }

            private void OnAppThreadExit(object sender, EventArgs e)
            {
                this.Dispose(true);
            }

            [PrePrepareMethod]
            private void OnDomainUnload(object sender, EventArgs e)
            {
                this.RevokeComponent();
                ExitDomain();
            }

            internal void OnThreadException(Exception t)
            {
                if (!this.GetState(4))
                {
                    this.SetState(4, true);
                    try
                    {
                        WarningException exception;
                        if (this.threadExceptionHandler != null)
                        {
                            this.threadExceptionHandler(Thread.CurrentThread, new ThreadExceptionEventArgs(t));
                        }
                        else if (SystemInformation.UserInteractive)
                        {
                            ThreadExceptionDialog dialog = new ThreadExceptionDialog(t);
                            DialogResult oK = DialogResult.OK;
                            System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                            try
                            {
                                oK = dialog.ShowDialog();
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                                dialog.Dispose();
                            }
                            switch (oK)
                            {
                                case DialogResult.Abort:
                                    Application.ExitInternal();
                                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                                    Environment.Exit(0);
                                    break;

                                case DialogResult.Yes:
                                    goto Label_0086;
                            }
                        }
                        return;
                    Label_0086:
                        exception = t as WarningException;
                        if (exception != null)
                        {
                            Help.ShowHelp(null, exception.HelpUrl, exception.HelpTopic);
                        }
                    }
                    finally
                    {
                        this.SetState(4, false);
                    }
                }
            }

            internal void PostQuit()
            {
                System.Windows.Forms.UnsafeNativeMethods.PostThreadMessage(this.id, 0x12, IntPtr.Zero, IntPtr.Zero);
                this.SetState(8, true);
            }

            internal bool PreTranslateMessage(ref System.Windows.Forms.NativeMethods.MSG msg)
            {
                bool modified = false;
                if (this.ProcessFilters(ref msg, out modified))
                {
                    return true;
                }
                if ((msg.message >= 0x100) && (msg.message <= 0x108))
                {
                    if (msg.message == 0x102)
                    {
                        int num = 0x1460000;
                        if (((((int) ((long) msg.wParam)) == 3) && ((((int) ((long) msg.lParam)) & num) == num)) && Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }
                    Control target = Control.FromChildHandleInternal(msg.hwnd);
                    bool flag2 = false;
                    Message message = Message.Create(msg.hwnd, msg.message, msg.wParam, msg.lParam);
                    if (target != null)
                    {
                        if (NativeWindow.WndProcShouldBeDebuggable)
                        {
                            if (Control.PreProcessControlMessageInternal(target, ref message) == PreProcessControlState.MessageProcessed)
                            {
                                flag2 = true;
                            }
                        }
                        else
                        {
                            try
                            {
                                if (Control.PreProcessControlMessageInternal(target, ref message) == PreProcessControlState.MessageProcessed)
                                {
                                    flag2 = true;
                                }
                            }
                            catch (Exception exception)
                            {
                                this.OnThreadException(exception);
                            }
                        }
                    }
                    else
                    {
                        IntPtr ancestor = System.Windows.Forms.UnsafeNativeMethods.GetAncestor(new HandleRef(null, msg.hwnd), 2);
                        if ((ancestor != IntPtr.Zero) && System.Windows.Forms.UnsafeNativeMethods.IsDialogMessage(new HandleRef(null, ancestor), ref msg))
                        {
                            return true;
                        }
                    }
                    msg.wParam = message.WParam;
                    msg.lParam = message.LParam;
                    if (flag2)
                    {
                        return true;
                    }
                }
                return false;
            }

            internal bool ProcessFilters(ref System.Windows.Forms.NativeMethods.MSG msg, out bool modified)
            {
                modified = false;
                if ((this.messageFilters != null) && !this.GetState(0x10))
                {
                    if (this.messageFilters.Count > 0)
                    {
                        this.messageFilterSnapshot = new IMessageFilter[this.messageFilters.Count];
                        this.messageFilters.CopyTo(this.messageFilterSnapshot);
                    }
                    else
                    {
                        this.messageFilterSnapshot = null;
                    }
                    this.SetState(0x10, true);
                }
                if (this.messageFilterSnapshot != null)
                {
                    int length = this.messageFilterSnapshot.Length;
                    Message m = Message.Create(msg.hwnd, msg.message, msg.wParam, msg.lParam);
                    for (int i = 0; i < length; i++)
                    {
                        IMessageFilter filter = this.messageFilterSnapshot[i];
                        bool flag2 = filter.PreFilterMessage(ref m);
                        if (filter is IMessageModifyAndFilter)
                        {
                            msg.hwnd = m.HWnd;
                            msg.message = m.Msg;
                            msg.wParam = m.WParam;
                            msg.lParam = m.LParam;
                            modified = true;
                        }
                        if (flag2)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            internal void RegisterMessageLoop(Application.MessageLoopCallback callback)
            {
                this.messageLoopCallback = callback;
            }

            internal void RemoveMessageFilter(IMessageFilter f)
            {
                if (this.messageFilters != null)
                {
                    this.SetState(0x10, false);
                    this.messageFilters.Remove(f);
                }
            }

            private void RevokeComponent()
            {
                if ((this.componentManager != null) && (this.componentID != -1))
                {
                    int componentID = this.componentID;
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = this.componentManager;
                    try
                    {
                        componentManager.FRevokeComponent((IntPtr) componentID);
                        if (Marshal.IsComObject(componentManager))
                        {
                            Marshal.ReleaseComObject(componentManager);
                        }
                    }
                    finally
                    {
                        this.componentManager = null;
                        this.componentID = -1;
                    }
                }
            }

            internal void RunMessageLoop(int reason, System.Windows.Forms.ApplicationContext context)
            {
                IntPtr zero = IntPtr.Zero;
                if (Application.useVisualStyles)
                {
                    zero = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                }
                try
                {
                    this.RunMessageLoopInner(reason, context);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(zero);
                }
            }

            private void RunMessageLoopInner(int reason, System.Windows.Forms.ApplicationContext context)
            {
                if ((reason == 4) && !SystemInformation.UserInteractive)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("CantShowModalOnNonInteractive"));
                }
                if (reason == -1)
                {
                    this.SetState(8, false);
                }
                if (totalMessageLoopCount++ == 0)
                {
                    baseLoopReason = reason;
                }
                this.messageLoopCount++;
                if (reason == -1)
                {
                    if (this.messageLoopCount != 1)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("CantNestMessageLoops"));
                    }
                    this.applicationContext = context;
                    this.applicationContext.ThreadExit += new EventHandler(this.OnAppThreadExit);
                    if (this.applicationContext.MainForm != null)
                    {
                        this.applicationContext.MainForm.Visible = true;
                    }
                }
                Form currentForm = this.currentForm;
                if (context != null)
                {
                    this.currentForm = context.MainForm;
                }
                bool flag = false;
                bool flag2 = false;
                HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
                if (reason == -2)
                {
                    flag2 = true;
                }
                if ((reason == 4) || (reason == 5))
                {
                    flag = true;
                    bool enable = (this.currentForm != null) && this.currentForm.Enabled;
                    this.BeginModalMessageLoop(context);
                    hWnd = new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this.currentForm, this.currentForm.Handle), -8));
                    if (hWnd.Handle != IntPtr.Zero)
                    {
                        if (System.Windows.Forms.SafeNativeMethods.IsWindowEnabled(hWnd))
                        {
                            System.Windows.Forms.SafeNativeMethods.EnableWindow(hWnd, false);
                        }
                        else
                        {
                            hWnd = new HandleRef(null, IntPtr.Zero);
                        }
                    }
                    if (((this.currentForm != null) && this.currentForm.IsHandleCreated) && (System.Windows.Forms.SafeNativeMethods.IsWindowEnabled(new HandleRef(this.currentForm, this.currentForm.Handle)) != enable))
                    {
                        System.Windows.Forms.SafeNativeMethods.EnableWindow(new HandleRef(this.currentForm, this.currentForm.Handle), enable);
                    }
                }
                try
                {
                    if (this.messageLoopCount == 1)
                    {
                        WindowsFormsSynchronizationContext.InstallIfNeeded();
                    }
                    if (flag && (this.currentForm != null))
                    {
                        this.currentForm.Visible = true;
                    }
                    if ((!flag && !flag2) || (this.ComponentManager is System.Windows.Forms.Application.ComponentManager))
                    {
                        this.ComponentManager.FPushMessageLoop((IntPtr) this.componentID, reason, 0);
                    }
                    else if ((reason == 2) || (reason == -2))
                    {
                        this.LocalModalMessageLoop(null);
                    }
                    else
                    {
                        this.LocalModalMessageLoop(this.currentForm);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.EndModalMessageLoop(context);
                        if (hWnd.Handle != IntPtr.Zero)
                        {
                            System.Windows.Forms.SafeNativeMethods.EnableWindow(hWnd, true);
                        }
                    }
                    this.currentForm = currentForm;
                    totalMessageLoopCount--;
                    this.messageLoopCount--;
                    if (this.messageLoopCount == 0)
                    {
                        WindowsFormsSynchronizationContext.Uninstall(false);
                    }
                    if (reason == -1)
                    {
                        this.Dispose(true);
                    }
                    else if ((this.messageLoopCount == 0) && (this.componentManager != null))
                    {
                        this.RevokeComponent();
                    }
                }
            }

            internal void SetCulture(CultureInfo culture)
            {
                if ((culture != null) && (culture.LCID != System.Windows.Forms.SafeNativeMethods.GetThreadLocale()))
                {
                    System.Windows.Forms.SafeNativeMethods.SetThreadLocale(culture.LCID);
                }
            }

            private void SetState(int bit, bool value)
            {
                if (value)
                {
                    this.threadState |= bit;
                }
                else
                {
                    this.threadState &= ~bit;
                }
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FContinueMessageLoop(int reason, int pvLoopData, System.Windows.Forms.NativeMethods.MSG[] msgPeeked)
            {
                bool flag = true;
                if ((msgPeeked == null) && this.GetState(8))
                {
                    return false;
                }
                switch (reason)
                {
                    case -2:
                    case 2:
                        if (!System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref this.tempMsg, System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0))
                        {
                            flag = false;
                        }
                        return flag;

                    case -1:
                    case 0:
                    case 3:
                        return flag;

                    case 1:
                        int num;
                        System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow()), out num);
                        if (num == System.Windows.Forms.SafeNativeMethods.GetCurrentProcessId())
                        {
                            flag = false;
                        }
                        return flag;

                    case 4:
                    case 5:
                        return (((this.currentForm != null) && !this.currentForm.CheckCloseDialog(false)) && flag);
                }
                return flag;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FDebugMessage(IntPtr hInst, int msg, IntPtr wparam, IntPtr lparam)
            {
                return false;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FDoIdle(int grfidlef)
            {
                if (this.idleHandler != null)
                {
                    this.idleHandler(Thread.CurrentThread, EventArgs.Empty);
                }
                return false;
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FPreTranslateMessage(ref System.Windows.Forms.NativeMethods.MSG msg)
            {
                return this.PreTranslateMessage(ref msg);
            }

            bool System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FQueryTerminate(bool fPromptUser)
            {
                return true;
            }

            IntPtr System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.HwndGetWindow(int dwWhich, int dwReserved)
            {
                return IntPtr.Zero;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.OnActivationChange(System.Windows.Forms.UnsafeNativeMethods.IMsoComponent component, bool fSameComponent, int pcrinfo, bool fHostIsActivating, int pchostinfo, int dwReserved)
            {
            }

            void System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.OnAppActivate(bool fActive, int dwOtherThreadID)
            {
            }

            void System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.OnEnterState(int uStateID, bool fEnter)
            {
                if (!this.ourModalLoop && (uStateID == 1))
                {
                    if (fEnter)
                    {
                        this.DisableWindowsForModalLoop(true, null);
                    }
                    else
                    {
                        this.EnableWindowsForModalLoop(true, null);
                    }
                }
            }

            void System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.OnLoseActivation()
            {
            }

            void System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.Terminate()
            {
                if ((this.messageLoopCount > 0) && !(this.ComponentManager is System.Windows.Forms.Application.ComponentManager))
                {
                    this.messageLoopCount--;
                }
                this.Dispose(false);
            }

            internal void TrackInput(bool track)
            {
                if (track != this.GetState(0x20))
                {
                    System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = this.ComponentManager;
                    if ((componentManager != null) && !(componentManager is System.Windows.Forms.Application.ComponentManager))
                    {
                        componentManager.FSetTrackingComponent((IntPtr) this.componentID, track);
                        this.SetState(0x20, track);
                    }
                }
            }

            internal Control ActivatingControl
            {
                get
                {
                    if ((this.activatingControlRef != null) && this.activatingControlRef.IsAlive)
                    {
                        return (this.activatingControlRef.Target as Control);
                    }
                    return null;
                }
                set
                {
                    if (value != null)
                    {
                        this.activatingControlRef = new WeakReference(value);
                    }
                    else
                    {
                        this.activatingControlRef = null;
                    }
                }
            }

            public System.Windows.Forms.ApplicationContext ApplicationContext
            {
                get
                {
                    return this.applicationContext;
                }
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager ComponentManager
            {
                get
                {
                    if (this.componentManager == null)
                    {
                        if (this.fetchingComponentManager)
                        {
                            return null;
                        }
                        this.fetchingComponentManager = true;
                        try
                        {
                            System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager componentManager = null;
                            Application.OleRequired();
                            IntPtr zero = IntPtr.Zero;
                            if (System.Windows.Forms.NativeMethods.Succeeded(System.Windows.Forms.UnsafeNativeMethods.CoRegisterMessageFilter(System.Windows.Forms.NativeMethods.NullHandleRef, ref zero)) && (zero != IntPtr.Zero))
                            {
                                IntPtr oldMsgFilter = IntPtr.Zero;
                                System.Windows.Forms.UnsafeNativeMethods.CoRegisterMessageFilter(new HandleRef(null, zero), ref oldMsgFilter);
                                object objectForIUnknown = Marshal.GetObjectForIUnknown(zero);
                                Marshal.Release(zero);
                                System.Windows.Forms.UnsafeNativeMethods.IOleServiceProvider provider = objectForIUnknown as System.Windows.Forms.UnsafeNativeMethods.IOleServiceProvider;
                                if (provider != null)
                                {
                                    try
                                    {
                                        IntPtr ppvObject = IntPtr.Zero;
                                        Guid guidService = new Guid("000C060B-0000-0000-C000-000000000046");
                                        Guid riid = new Guid("{000C0601-0000-0000-C000-000000000046}");
                                        int hr = provider.QueryService(ref guidService, ref riid, out ppvObject);
                                        if (System.Windows.Forms.NativeMethods.Succeeded(hr) && (ppvObject != IntPtr.Zero))
                                        {
                                            IntPtr ptr4;
                                            try
                                            {
                                                Guid gUID = typeof(System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager).GUID;
                                                hr = Marshal.QueryInterface(ppvObject, ref gUID, out ptr4);
                                            }
                                            finally
                                            {
                                                Marshal.Release(ppvObject);
                                            }
                                            if (System.Windows.Forms.NativeMethods.Succeeded(hr) && (ptr4 != IntPtr.Zero))
                                            {
                                                try
                                                {
                                                    componentManager = ComponentManagerBroker.GetComponentManager(ptr4);
                                                }
                                                finally
                                                {
                                                    Marshal.Release(ptr4);
                                                }
                                            }
                                            if (componentManager != null)
                                            {
                                                if (zero == ppvObject)
                                                {
                                                    objectForIUnknown = null;
                                                }
                                                this.externalComponentManager = true;
                                                AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.OnDomainUnload);
                                                AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.OnDomainUnload);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                if ((objectForIUnknown != null) && Marshal.IsComObject(objectForIUnknown))
                                {
                                    Marshal.ReleaseComObject(objectForIUnknown);
                                }
                            }
                            if (componentManager == null)
                            {
                                componentManager = new System.Windows.Forms.Application.ComponentManager();
                                this.externalComponentManager = false;
                            }
                            if ((componentManager != null) && (this.componentID == -1))
                            {
                                IntPtr ptr5;
                                System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT pcrinfo = new System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT {
                                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT)),
                                    uIdleTimeInterval = 0,
                                    grfcrf = 9,
                                    grfcadvf = 1
                                };
                                bool flag = componentManager.FRegisterComponent(this, pcrinfo, out ptr5);
                                this.componentID = (int) ((long) ptr5);
                                if (flag && !(componentManager is System.Windows.Forms.Application.ComponentManager))
                                {
                                    this.messageLoopCount++;
                                }
                                this.componentManager = componentManager;
                            }
                        }
                        finally
                        {
                            this.fetchingComponentManager = false;
                        }
                    }
                    return this.componentManager;
                }
            }

            internal bool CustomThreadExceptionHandlerAttached
            {
                get
                {
                    return (this.threadExceptionHandler != null);
                }
            }

            internal Control MarshalingControl
            {
                get
                {
                    lock (this)
                    {
                        if (this.marshalingControl == null)
                        {
                            this.marshalingControl = new System.Windows.Forms.Application.MarshalingControl();
                        }
                        return this.marshalingControl;
                    }
                }
            }

            internal System.Windows.Forms.Application.ParkingWindow ParkingWindow
            {
                get
                {
                    lock (this)
                    {
                        if (this.parkingWindow == null)
                        {
                            System.Windows.Forms.IntSecurity.ManipulateWndProcAndHandles.Assert();
                            try
                            {
                                this.parkingWindow = new System.Windows.Forms.Application.ParkingWindow();
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                        return this.parkingWindow;
                    }
                }
            }
        }

        private sealed class ThreadWindows
        {
            private IntPtr activeHwnd;
            private IntPtr focusedHwnd;
            private bool onlyWinForms = true;
            internal Application.ThreadWindows previousThreadWindows;
            private int windowCount;
            private IntPtr[] windows = new IntPtr[0x10];

            internal ThreadWindows(bool onlyWinForms)
            {
                this.onlyWinForms = onlyWinForms;
                System.Windows.Forms.UnsafeNativeMethods.EnumThreadWindows(System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId(), new System.Windows.Forms.NativeMethods.EnumThreadWindowsCallback(this.Callback), System.Windows.Forms.NativeMethods.NullHandleRef);
            }

            private bool Callback(IntPtr hWnd, IntPtr lparam)
            {
                if (System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(null, hWnd)) && System.Windows.Forms.SafeNativeMethods.IsWindowEnabled(new HandleRef(null, hWnd)))
                {
                    bool flag = true;
                    if (this.onlyWinForms && (Control.FromHandleInternal(hWnd) == null))
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        if (this.windowCount == this.windows.Length)
                        {
                            IntPtr[] destinationArray = new IntPtr[this.windowCount * 2];
                            Array.Copy(this.windows, 0, destinationArray, 0, this.windowCount);
                            this.windows = destinationArray;
                        }
                        this.windows[this.windowCount++] = hWnd;
                    }
                }
                return true;
            }

            internal void Dispose()
            {
                for (int i = 0; i < this.windowCount; i++)
                {
                    IntPtr handle = this.windows[i];
                    if (System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, handle)))
                    {
                        Control control = Control.FromHandleInternal(handle);
                        if (control != null)
                        {
                            control.Dispose();
                        }
                    }
                }
            }

            internal void Enable(bool state)
            {
                if (!this.onlyWinForms && !state)
                {
                    this.activeHwnd = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
                    Control activatingControl = Application.ThreadContext.FromCurrent().ActivatingControl;
                    if (activatingControl != null)
                    {
                        this.focusedHwnd = activatingControl.Handle;
                    }
                    else
                    {
                        this.focusedHwnd = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                    }
                }
                for (int i = 0; i < this.windowCount; i++)
                {
                    IntPtr handle = this.windows[i];
                    if (System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, handle)))
                    {
                        System.Windows.Forms.SafeNativeMethods.EnableWindow(new HandleRef(null, handle), state);
                    }
                }
                if (!this.onlyWinForms && state)
                {
                    if ((this.activeHwnd != IntPtr.Zero) && System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, this.activeHwnd)))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetActiveWindow(new HandleRef(null, this.activeHwnd));
                    }
                    if ((this.focusedHwnd != IntPtr.Zero) && System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, this.focusedHwnd)))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(null, this.focusedHwnd));
                    }
                }
            }
        }
    }
}

