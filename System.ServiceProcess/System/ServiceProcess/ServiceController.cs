namespace System.ServiceProcess
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceProcess.Design;
    using System.Text;
    using System.Threading;

    [ServiceProcessDescription("ServiceControllerDesc"), Designer("System.ServiceProcess.Design.ServiceControllerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ServiceController : Component
    {
        private bool browseGranted;
        private int commandsAccepted;
        private bool controlGranted;
        private ServiceController[] dependentServices;
        private string displayName;
        private const int DISPLAYNAMEBUFFERSIZE = 0x100;
        private bool disposed;
        private string eitherName;
        private static int environment = UnknownEnvironment;
        private string machineName;
        private string name;
        private static readonly int NonNtEnvironment = 2;
        private static readonly int NtEnvironment = 1;
        private static object s_InternalSyncObject;
        private IntPtr serviceManagerHandle;
        private ServiceController[] servicesDependedOn;
        private ServiceControllerStatus status;
        private bool statusGenerated;
        private int type;
        private static readonly int UnknownEnvironment = 0;

        public ServiceController()
        {
            this.machineName = ".";
            this.name = "";
            this.displayName = "";
            this.eitherName = "";
            this.type = 0x13f;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceController(string name) : this(name, ".")
        {
        }

        internal ServiceController(string machineName, System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS status)
        {
            this.machineName = ".";
            this.name = "";
            this.displayName = "";
            this.eitherName = "";
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(Res.GetString("BadMachineName", new object[] { machineName }));
            }
            this.machineName = machineName;
            this.name = status.serviceName;
            this.displayName = status.displayName;
            this.commandsAccepted = status.controlsAccepted;
            this.status = (ServiceControllerStatus) status.currentState;
            this.type = status.serviceType;
            this.statusGenerated = true;
        }

        internal ServiceController(string machineName, System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS status)
        {
            this.machineName = ".";
            this.name = "";
            this.displayName = "";
            this.eitherName = "";
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(Res.GetString("BadMachineName", new object[] { machineName }));
            }
            this.machineName = machineName;
            this.name = status.serviceName;
            this.displayName = status.displayName;
            this.commandsAccepted = status.controlsAccepted;
            this.status = (ServiceControllerStatus) status.currentState;
            this.type = status.serviceType;
            this.statusGenerated = true;
        }

        public ServiceController(string name, string machineName)
        {
            this.machineName = ".";
            this.name = "";
            this.displayName = "";
            this.eitherName = "";
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(Res.GetString("BadMachineName", new object[] { machineName }));
            }
            if ((name == null) || (name.Length == 0))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "name", name }));
            }
            this.machineName = machineName;
            this.eitherName = name;
            this.type = 0x13f;
        }

        private static void CheckEnvironment()
        {
            if (environment == UnknownEnvironment)
            {
                lock (InternalSyncObject)
                {
                    if (environment == UnknownEnvironment)
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            environment = NtEnvironment;
                        }
                        else
                        {
                            environment = NonNtEnvironment;
                        }
                    }
                }
            }
            if (environment == NonNtEnvironment)
            {
                throw new PlatformNotSupportedException(Res.GetString("CantControlOnWin9x"));
            }
        }

        public void Close()
        {
            if (this.serviceManagerHandle != IntPtr.Zero)
            {
                SafeNativeMethods.CloseServiceHandle(this.serviceManagerHandle);
            }
            this.serviceManagerHandle = IntPtr.Zero;
            this.statusGenerated = false;
            this.type = 0x13f;
            this.browseGranted = false;
            this.controlGranted = false;
        }

        public unsafe void Continue()
        {
            if (!this.controlGranted)
            {
                new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, this.machineName, this.ServiceName).Demand();
                this.controlGranted = true;
            }
            IntPtr serviceHandle = this.GetServiceHandle(0x40);
            try
            {
                System.ServiceProcess.NativeMethods.SERVICE_STATUS pStatus = new System.ServiceProcess.NativeMethods.SERVICE_STATUS();
                if (!System.ServiceProcess.UnsafeNativeMethods.ControlService(serviceHandle, 3, &pStatus))
                {
                    Exception innerException = CreateSafeWin32Exception();
                    throw new InvalidOperationException(Res.GetString("ResumeService", new object[] { this.ServiceName, this.MachineName }), innerException);
                }
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(serviceHandle);
            }
        }

        private static Win32Exception CreateSafeWin32Exception()
        {
            Win32Exception exception = null;
            new SecurityPermission(PermissionState.Unrestricted).Assert();
            try
            {
                exception = new Win32Exception();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return exception;
        }

        protected override void Dispose(bool disposing)
        {
            this.Close();
            this.disposed = true;
            base.Dispose(disposing);
        }

        public unsafe void ExecuteCommand(int command)
        {
            if (!this.controlGranted)
            {
                new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, this.machineName, this.ServiceName).Demand();
                this.controlGranted = true;
            }
            IntPtr serviceHandle = this.GetServiceHandle(0x100);
            try
            {
                System.ServiceProcess.NativeMethods.SERVICE_STATUS pStatus = new System.ServiceProcess.NativeMethods.SERVICE_STATUS();
                if (!System.ServiceProcess.UnsafeNativeMethods.ControlService(serviceHandle, command, &pStatus))
                {
                    Exception innerException = CreateSafeWin32Exception();
                    throw new InvalidOperationException(Res.GetString("ControlService", new object[] { this.ServiceName, this.MachineName }), innerException);
                }
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(serviceHandle);
            }
        }

        private void GenerateNames()
        {
            if (this.machineName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("NoMachineName"));
            }
            this.GetDataBaseHandleWithConnectAccess();
            if (this.name.Length == 0)
            {
                string eitherName = this.eitherName;
                if (eitherName.Length == 0)
                {
                    eitherName = this.displayName;
                }
                if (eitherName.Length == 0)
                {
                    throw new InvalidOperationException(Res.GetString("NoGivenName"));
                }
                int capacity = 0x100;
                StringBuilder shortName = new StringBuilder(capacity);
                if (!SafeNativeMethods.GetServiceKeyName(this.serviceManagerHandle, eitherName, shortName, ref capacity))
                {
                    bool flag = SafeNativeMethods.GetServiceDisplayName(this.serviceManagerHandle, eitherName, shortName, ref capacity);
                    if (!flag && (capacity >= 0x100))
                    {
                        shortName = new StringBuilder(++capacity);
                        flag = SafeNativeMethods.GetServiceDisplayName(this.serviceManagerHandle, eitherName, shortName, ref capacity);
                    }
                    if (!flag)
                    {
                        Exception innerException = CreateSafeWin32Exception();
                        throw new InvalidOperationException(Res.GetString("NoService", new object[] { eitherName, this.machineName }), innerException);
                    }
                    this.name = eitherName;
                    this.displayName = shortName.ToString();
                    this.eitherName = "";
                }
                else
                {
                    this.name = shortName.ToString();
                    this.displayName = eitherName;
                    this.eitherName = "";
                }
            }
            if (this.displayName.Length == 0)
            {
                int num2 = 0x100;
                StringBuilder displayName = new StringBuilder(num2);
                bool flag2 = SafeNativeMethods.GetServiceDisplayName(this.serviceManagerHandle, this.name, displayName, ref num2);
                if (!flag2 && (num2 >= 0x100))
                {
                    displayName = new StringBuilder(++num2);
                    flag2 = SafeNativeMethods.GetServiceDisplayName(this.serviceManagerHandle, this.name, displayName, ref num2);
                }
                if (!flag2)
                {
                    Exception exception2 = CreateSafeWin32Exception();
                    throw new InvalidOperationException(Res.GetString("NoDisplayName", new object[] { this.name, this.machineName }), exception2);
                }
                this.displayName = displayName.ToString();
            }
        }

        private unsafe void GenerateStatus()
        {
            if (!this.statusGenerated)
            {
                if (!this.browseGranted)
                {
                    new ServiceControllerPermission(ServiceControllerPermissionAccess.Browse, this.machineName, this.ServiceName).Demand();
                    this.browseGranted = true;
                }
                IntPtr serviceHandle = this.GetServiceHandle(4);
                try
                {
                    System.ServiceProcess.NativeMethods.SERVICE_STATUS pStatus = new System.ServiceProcess.NativeMethods.SERVICE_STATUS();
                    if (!System.ServiceProcess.UnsafeNativeMethods.QueryServiceStatus(serviceHandle, &pStatus))
                    {
                        throw CreateSafeWin32Exception();
                    }
                    this.commandsAccepted = pStatus.controlsAccepted;
                    this.status = (ServiceControllerStatus) pStatus.currentState;
                    this.type = pStatus.serviceType;
                    this.statusGenerated = true;
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(serviceHandle);
                }
            }
        }

        private static IntPtr GetDataBaseHandleWithAccess(string machineName, int serviceControlManaqerAccess)
        {
            CheckEnvironment();
            IntPtr zero = IntPtr.Zero;
            if (machineName.Equals(".") || (machineName.Length == 0))
            {
                zero = SafeNativeMethods.OpenSCManager(null, null, serviceControlManaqerAccess);
            }
            else
            {
                zero = SafeNativeMethods.OpenSCManager(machineName, null, serviceControlManaqerAccess);
            }
            if (zero == IntPtr.Zero)
            {
                Exception innerException = CreateSafeWin32Exception();
                throw new InvalidOperationException(Res.GetString("OpenSC", new object[] { machineName }), innerException);
            }
            return zero;
        }

        private void GetDataBaseHandleWithConnectAccess()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (this.serviceManagerHandle == IntPtr.Zero)
            {
                this.serviceManagerHandle = GetDataBaseHandleWithAccess(this.MachineName, 1);
            }
        }

        private static IntPtr GetDataBaseHandleWithEnumerateAccess(string machineName)
        {
            return GetDataBaseHandleWithAccess(machineName, 4);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceController[] GetDevices()
        {
            return GetDevices(".");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceController[] GetDevices(string machineName)
        {
            return GetServicesOfType(machineName, 11);
        }

        private IntPtr GetServiceHandle(int desiredAccess)
        {
            this.GetDataBaseHandleWithConnectAccess();
            IntPtr ptr = System.ServiceProcess.UnsafeNativeMethods.OpenService(this.serviceManagerHandle, this.ServiceName, desiredAccess);
            if (ptr == IntPtr.Zero)
            {
                Exception innerException = CreateSafeWin32Exception();
                throw new InvalidOperationException(Res.GetString("OpenService", new object[] { this.ServiceName, this.MachineName }), innerException);
            }
            return ptr;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceController[] GetServices()
        {
            return GetServices(".");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceController[] GetServices(string machineName)
        {
            return GetServicesOfType(machineName, 0x30);
        }

        private static System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS[] GetServicesInGroup(string machineName, string group)
        {
            System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS[] enum_service_status_processArray;
            IntPtr zero = IntPtr.Zero;
            IntPtr status = IntPtr.Zero;
            int resumeHandle = 0;
            try
            {
                int num;
                int num2;
                zero = GetDataBaseHandleWithEnumerateAccess(machineName);
                System.ServiceProcess.UnsafeNativeMethods.EnumServicesStatusEx(zero, 0, 0x30, 3, IntPtr.Zero, 0, out num, out num2, ref resumeHandle, group);
                status = Marshal.AllocHGlobal((IntPtr) num);
                System.ServiceProcess.UnsafeNativeMethods.EnumServicesStatusEx(zero, 0, 0x30, 3, status, num, out num, out num2, ref resumeHandle, group);
                int num4 = num2;
                enum_service_status_processArray = new System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS[num4];
                for (int i = 0; i < num4; i++)
                {
                    IntPtr ptr = (IntPtr) (((long) status) + (i * Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS))));
                    System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS structure = new System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS();
                    Marshal.PtrToStructure(ptr, structure);
                    enum_service_status_processArray[i] = structure;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(status);
                if (zero != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseServiceHandle(zero);
                }
            }
            return enum_service_status_processArray;
        }

        private static ServiceController[] GetServicesOfType(string machineName, int serviceType)
        {
            ServiceController[] controllerArray;
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(Res.GetString("BadMachineName", new object[] { machineName }));
            }
            new ServiceControllerPermission(ServiceControllerPermissionAccess.Browse, machineName, "*").Demand();
            CheckEnvironment();
            IntPtr zero = IntPtr.Zero;
            IntPtr status = IntPtr.Zero;
            int resumeHandle = 0;
            try
            {
                int num;
                int num2;
                zero = GetDataBaseHandleWithEnumerateAccess(machineName);
                System.ServiceProcess.UnsafeNativeMethods.EnumServicesStatus(zero, serviceType, 3, IntPtr.Zero, 0, out num, out num2, ref resumeHandle);
                status = Marshal.AllocHGlobal((IntPtr) num);
                System.ServiceProcess.UnsafeNativeMethods.EnumServicesStatus(zero, serviceType, 3, status, num, out num, out num2, ref resumeHandle);
                int num4 = num2;
                controllerArray = new ServiceController[num4];
                for (int i = 0; i < num4; i++)
                {
                    IntPtr ptr = (IntPtr) (((long) status) + (i * Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS))));
                    System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS structure = new System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS();
                    Marshal.PtrToStructure(ptr, structure);
                    controllerArray[i] = new ServiceController(machineName, structure);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(status);
                if (zero != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseServiceHandle(zero);
                }
            }
            return controllerArray;
        }

        public unsafe void Pause()
        {
            if (!this.controlGranted)
            {
                new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, this.machineName, this.ServiceName).Demand();
                this.controlGranted = true;
            }
            IntPtr serviceHandle = this.GetServiceHandle(0x40);
            try
            {
                System.ServiceProcess.NativeMethods.SERVICE_STATUS pStatus = new System.ServiceProcess.NativeMethods.SERVICE_STATUS();
                if (!System.ServiceProcess.UnsafeNativeMethods.ControlService(serviceHandle, 2, &pStatus))
                {
                    Exception innerException = CreateSafeWin32Exception();
                    throw new InvalidOperationException(Res.GetString("PauseService", new object[] { this.ServiceName, this.MachineName }), innerException);
                }
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(serviceHandle);
            }
        }

        public void Refresh()
        {
            this.statusGenerated = false;
            this.dependentServices = null;
            this.servicesDependedOn = null;
        }

        public void Start()
        {
            this.Start(new string[0]);
        }

        public void Start(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            if (!this.controlGranted)
            {
                new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, this.machineName, this.ServiceName).Demand();
                this.controlGranted = true;
            }
            IntPtr serviceHandle = this.GetServiceHandle(0x10);
            try
            {
                IntPtr[] ptrArray = new IntPtr[args.Length];
                int index = 0;
                try
                {
                    index = 0;
                    while (index < args.Length)
                    {
                        if (args[index] == null)
                        {
                            throw new ArgumentNullException(Res.GetString("ArgsCantBeNull"), "args");
                        }
                        ptrArray[index] = Marshal.StringToHGlobalUni(args[index]);
                        index++;
                    }
                }
                catch
                {
                    for (int i = 0; i < index; i++)
                    {
                        Marshal.FreeHGlobal(ptrArray[index]);
                    }
                    throw;
                }
                GCHandle handle = new GCHandle();
                try
                {
                    handle = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
                    if (!System.ServiceProcess.UnsafeNativeMethods.StartService(serviceHandle, args.Length, handle.AddrOfPinnedObject()))
                    {
                        Exception innerException = CreateSafeWin32Exception();
                        throw new InvalidOperationException(Res.GetString("CannotStart", new object[] { this.ServiceName, this.MachineName }), innerException);
                    }
                }
                finally
                {
                    for (index = 0; index < args.Length; index++)
                    {
                        Marshal.FreeHGlobal(ptrArray[index]);
                    }
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(serviceHandle);
            }
        }

        public unsafe void Stop()
        {
            if (!this.controlGranted)
            {
                new ServiceControllerPermission(ServiceControllerPermissionAccess.Control, this.machineName, this.ServiceName).Demand();
                this.controlGranted = true;
            }
            IntPtr serviceHandle = this.GetServiceHandle(0x20);
            try
            {
                for (int i = 0; i < this.DependentServices.Length; i++)
                {
                    ServiceController controller = this.DependentServices[i];
                    controller.Refresh();
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                    }
                }
                System.ServiceProcess.NativeMethods.SERVICE_STATUS pStatus = new System.ServiceProcess.NativeMethods.SERVICE_STATUS();
                if (!System.ServiceProcess.UnsafeNativeMethods.ControlService(serviceHandle, 1, &pStatus))
                {
                    Exception innerException = CreateSafeWin32Exception();
                    throw new InvalidOperationException(Res.GetString("StopService", new object[] { this.ServiceName, this.MachineName }), innerException);
                }
            }
            finally
            {
                SafeNativeMethods.CloseServiceHandle(serviceHandle);
            }
        }

        internal static bool ValidServiceName(string serviceName)
        {
            if (serviceName == null)
            {
                return false;
            }
            if ((serviceName.Length > 80) || (serviceName.Length == 0))
            {
                return false;
            }
            foreach (char ch in serviceName.ToCharArray())
            {
                switch (ch)
                {
                    case '\\':
                    case '/':
                        return false;
                }
            }
            return true;
        }

        public void WaitForStatus(ServiceControllerStatus desiredStatus)
        {
            this.WaitForStatus(desiredStatus, TimeSpan.MaxValue);
        }

        public void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            if (!Enum.IsDefined(typeof(ServiceControllerStatus), desiredStatus))
            {
                throw new InvalidEnumArgumentException("desiredStatus", (int) desiredStatus, typeof(ServiceControllerStatus));
            }
            DateTime utcNow = DateTime.UtcNow;
            this.Refresh();
            while (this.Status != desiredStatus)
            {
                if ((DateTime.UtcNow - utcNow) > timeout)
                {
                    throw new System.ServiceProcess.TimeoutException(Res.GetString("Timeout"));
                }
                Thread.Sleep(250);
                this.Refresh();
            }
        }

        [ServiceProcessDescription("SPCanPauseAndContinue"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanPauseAndContinue
        {
            get
            {
                this.GenerateStatus();
                return ((this.commandsAccepted & 2) != 0);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ServiceProcessDescription("SPCanShutdown")]
        public bool CanShutdown
        {
            get
            {
                this.GenerateStatus();
                return ((this.commandsAccepted & 4) != 0);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ServiceProcessDescription("SPCanStop")]
        public bool CanStop
        {
            get
            {
                this.GenerateStatus();
                return ((this.commandsAccepted & 1) != 0);
            }
        }

        [ServiceProcessDescription("SPDependentServices"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ServiceController[] DependentServices
        {
            get
            {
                if (!this.browseGranted)
                {
                    new ServiceControllerPermission(ServiceControllerPermissionAccess.Browse, this.machineName, this.ServiceName).Demand();
                    this.browseGranted = true;
                }
                if (this.dependentServices == null)
                {
                    IntPtr serviceHandle = this.GetServiceHandle(8);
                    try
                    {
                        int bytesNeeded = 0;
                        int numEnumerated = 0;
                        if (System.ServiceProcess.UnsafeNativeMethods.EnumDependentServices(serviceHandle, 3, IntPtr.Zero, 0, ref bytesNeeded, ref numEnumerated))
                        {
                            this.dependentServices = new ServiceController[0];
                            return this.dependentServices;
                        }
                        if (Marshal.GetLastWin32Error() != 0xea)
                        {
                            throw CreateSafeWin32Exception();
                        }
                        IntPtr ptr2 = Marshal.AllocHGlobal((IntPtr) bytesNeeded);
                        try
                        {
                            if (!System.ServiceProcess.UnsafeNativeMethods.EnumDependentServices(serviceHandle, 3, ptr2, bytesNeeded, ref bytesNeeded, ref numEnumerated))
                            {
                                throw CreateSafeWin32Exception();
                            }
                            this.dependentServices = new ServiceController[numEnumerated];
                            for (int i = 0; i < numEnumerated; i++)
                            {
                                System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS structure = new System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS();
                                IntPtr ptr = (IntPtr) (((long) ptr2) + (i * Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS))));
                                Marshal.PtrToStructure(ptr, structure);
                                this.dependentServices[i] = new ServiceController(this.MachineName, structure);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr2);
                        }
                    }
                    finally
                    {
                        SafeNativeMethods.CloseServiceHandle(serviceHandle);
                    }
                }
                return this.dependentServices;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ServiceProcessDescription("SPDisplayName"), ReadOnly(true)]
        public string DisplayName
        {
            get
            {
                if ((this.displayName.Length == 0) && ((this.eitherName.Length > 0) || (this.name.Length > 0)))
                {
                    this.GenerateNames();
                }
                return this.displayName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (string.Compare(value, this.displayName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.displayName = value;
                }
                else
                {
                    this.Close();
                    this.displayName = value;
                    this.name = "";
                }
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        [SettingsBindable(true), Browsable(false), ServiceProcessDescription("SPMachineName"), DefaultValue(".")]
        public string MachineName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.machineName;
            }
            set
            {
                if (!SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(Res.GetString("BadMachineName", new object[] { value }));
                }
                if (string.Compare(this.machineName, value, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.machineName = value;
                }
                else
                {
                    this.Close();
                    this.machineName = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public SafeHandle ServiceHandle
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                return new SafeServiceHandle(this.GetServiceHandle(0xf01ff), true);
            }
        }

        [ReadOnly(true), TypeConverter(typeof(ServiceNameConverter)), SettingsBindable(true), ServiceProcessDescription("SPServiceName"), DefaultValue("")]
        public string ServiceName
        {
            get
            {
                if ((this.name.Length == 0) && ((this.eitherName.Length > 0) || (this.displayName.Length > 0)))
                {
                    this.GenerateNames();
                }
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (string.Compare(value, this.name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.name = value;
                }
                else
                {
                    if (!ValidServiceName(value))
                    {
                        object[] args = new object[] { value, 80.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentException(Res.GetString("ServiceName", args));
                    }
                    this.Close();
                    this.name = value;
                    this.displayName = "";
                }
            }
        }

        [ServiceProcessDescription("SPServicesDependedOn"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ServiceController[] ServicesDependedOn
        {
            get
            {
                ServiceController[] servicesDependedOn;
                if (!this.browseGranted)
                {
                    new ServiceControllerPermission(ServiceControllerPermissionAccess.Browse, this.machineName, this.ServiceName).Demand();
                    this.browseGranted = true;
                }
                if (this.servicesDependedOn != null)
                {
                    return this.servicesDependedOn;
                }
                IntPtr serviceHandle = this.GetServiceHandle(1);
                try
                {
                    int bytesNeeded = 0;
                    if (System.ServiceProcess.UnsafeNativeMethods.QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out bytesNeeded))
                    {
                        this.servicesDependedOn = new ServiceController[0];
                        servicesDependedOn = this.servicesDependedOn;
                    }
                    else
                    {
                        if (Marshal.GetLastWin32Error() != 0x7a)
                        {
                            throw CreateSafeWin32Exception();
                        }
                        IntPtr ptr2 = Marshal.AllocHGlobal((IntPtr) bytesNeeded);
                        try
                        {
                            if (!System.ServiceProcess.UnsafeNativeMethods.QueryServiceConfig(serviceHandle, ptr2, bytesNeeded, out bytesNeeded))
                            {
                                throw CreateSafeWin32Exception();
                            }
                            System.ServiceProcess.NativeMethods.QUERY_SERVICE_CONFIG structure = new System.ServiceProcess.NativeMethods.QUERY_SERVICE_CONFIG();
                            Marshal.PtrToStructure(ptr2, structure);
                            char* lpDependencies = structure.lpDependencies;
                            Hashtable hashtable = new Hashtable();
                            if (lpDependencies != null)
                            {
                                StringBuilder builder = new StringBuilder();
                                while (lpDependencies[0] != '\0')
                                {
                                    builder.Append(lpDependencies[0]);
                                    lpDependencies++;
                                    if (lpDependencies[0] == '\0')
                                    {
                                        string key = builder.ToString();
                                        builder = new StringBuilder();
                                        lpDependencies++;
                                        if (key.StartsWith("+", StringComparison.Ordinal))
                                        {
                                            foreach (System.ServiceProcess.NativeMethods.ENUM_SERVICE_STATUS_PROCESS enum_service_status_process in GetServicesInGroup(this.machineName, key.Substring(1)))
                                            {
                                                if (!hashtable.Contains(enum_service_status_process.serviceName))
                                                {
                                                    hashtable.Add(enum_service_status_process.serviceName, new ServiceController(this.MachineName, enum_service_status_process));
                                                }
                                            }
                                        }
                                        else if (!hashtable.Contains(key))
                                        {
                                            hashtable.Add(key, new ServiceController(key, this.MachineName));
                                        }
                                    }
                                }
                            }
                            this.servicesDependedOn = new ServiceController[hashtable.Count];
                            hashtable.Values.CopyTo(this.servicesDependedOn, 0);
                            servicesDependedOn = this.servicesDependedOn;
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr2);
                        }
                    }
                }
                finally
                {
                    SafeNativeMethods.CloseServiceHandle(serviceHandle);
                }
                return servicesDependedOn;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ServiceProcessDescription("SPServiceType")]
        public System.ServiceProcess.ServiceType ServiceType
        {
            get
            {
                this.GenerateStatus();
                return (System.ServiceProcess.ServiceType) this.type;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ServiceProcessDescription("SPStatus")]
        public ServiceControllerStatus Status
        {
            get
            {
                this.GenerateStatus();
                return this.status;
            }
        }
    }
}

