namespace System.Deployment.Application
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Text;

    internal static class NativeMethods
    {
        private static Guid _clrRuntimeInfoGuid = new Guid(0xbd39d1d2, 0xba2f, 0x486a, 0x89, 0xb0, 180, 0xb0, 0xcb, 70, 0x68, 0x91);
        private static Guid _corRuntimeHostClsIdGuid = new Guid(0xcb2f6723, 0xab3a, 0x11d2, 0x9c, 0x40, 0, 0xc0, 0x4f, 0xa3, 10, 0x3e);
        private static Guid _corRuntimeHostInterfaceIdGuid = new Guid(0xcb2f6722, 0xab3a, 0x11d2, 0x9c, 0x40, 0, 0xc0, 0x4f, 0xa3, 10, 0x3e);
        private static Guid _metaHostClsId = new Guid(0x9280188d, 0xe8e, 0x4867, 0xb3, 12, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde);
        private static Guid _metaHostGuid = new Guid(0xd332db9e, 0xb9b3, 0x4125, 130, 7, 0xa1, 0x48, 0x84, 0xf5, 50, 0x16);
        private static Guid _metaHostPolicyClsIdGuid = new Guid(0x2ebcd49a, 0x1b47, 0x4a61, 0xb1, 0x3a, 0x4a, 3, 0x70, 30, 0x59, 0x4b);
        private static Guid _metaHostPolicyGuid = new Guid(0xe2190695, 0x77b2, 0x492e, 0x8e, 20, 0xc4, 0xb3, 0xa7, 0xfd, 0xd5, 0x93);
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        public const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        public const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool CloseHandle(HandleRef handle);
        [DllImport("Ole32.dll")]
        public static extern uint CoCreateInstance([In] ref Guid clsid, [MarshalAs(UnmanagedType.IUnknown)] object punkOuter, int context, [In] ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object o);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CommitUrlCacheEntry([In] string lpszUrlName, [In] string lpszLocalFileName, [In] long ExpireTime, [In] long LastModifiedTime, [In] uint CacheEntryType, [In] string lpHeaderInfo, [In] int dwHeaderSize, [In] string lpszFileExtension, [In] string lpszOriginalUrl);
        [DllImport("clr.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern void CorLaunchApplication(uint hostType, string applicationFullName, int manifestPathsCount, string[] manifestPaths, int activationDataCount, string[] activationData, PROCESS_INFORMATION processInformation);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern IntPtr CreateActCtxW([In] ACTCTXW actCtx);
        [DllImport("clr.dll", PreserveSig=false)]
        internal static extern void CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
        [DllImport("clr.dll", CharSet=CharSet.Auto, PreserveSig=false)]
        internal static extern void CreateAssemblyEnum(out IAssemblyEnum ppEnum, IApplicationContext pAppCtx, IAssemblyName pName, uint dwFlags, IntPtr pvReserved);
        [DllImport("clr.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
        internal static extern void CreateAssemblyNameObject(out IAssemblyName ppEnum, string szAssemblyName, uint dwFlags, IntPtr pvReserved);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool CreateUrlCacheEntry([In] string urlName, [In] int expectedFileSize, [In] string fileExtension, [Out] StringBuilder fileName, [In] int dwReserved);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        public static IAssemblyCache GetAssemblyCacheInterface(string CLRVersionString, bool FetchRuntimeHost, out CCorRuntimeHost RuntimeHost)
        {
            IClrMetaHostPolicy clrMetaHostPolicy = null;
            RuntimeHost = null;
            GetClrMetaHostPolicy(ref _metaHostPolicyClsIdGuid, ref _metaHostPolicyGuid, out clrMetaHostPolicy);
            if (clrMetaHostPolicy == null)
            {
                return null;
            }
            StringBuilder version = new StringBuilder("v", "v65535.65535.65535".Length);
            version.Append(CLRVersionString);
            int capacity = version.Capacity;
            int imageVersionLength = 0;
            int pdwConfigFlags = 0;
            IClrRuntimeInfo runtimeInfo = (IClrRuntimeInfo) clrMetaHostPolicy.GetRequestedRuntime(MetaHostPolicyFlags.MetaHostPolicyApplyUpgradePolicy, null, null, version, ref capacity, null, ref imageVersionLength, out pdwConfigFlags, _clrRuntimeInfoGuid);
            if (runtimeInfo == null)
            {
                return null;
            }
            CoInitializeEEDelegate delegateForFunctionPointer = (CoInitializeEEDelegate) Marshal.GetDelegateForFunctionPointer(runtimeInfo.GetProcAddress("CoInitializeEE"), typeof(CoInitializeEEDelegate));
            Marshal.ThrowExceptionForHR(delegateForFunctionPointer(0));
            if (FetchRuntimeHost)
            {
                RuntimeHost = new CCorRuntimeHost(runtimeInfo);
            }
            CreateAssemblyCacheDelegate delegate3 = (CreateAssemblyCacheDelegate) Marshal.GetDelegateForFunctionPointer(runtimeInfo.GetProcAddress("CreateAssemblyCache"), typeof(CreateAssemblyCacheDelegate));
            IAssemblyCache ppAsmCache = null;
            Marshal.ThrowExceptionForHR(delegate3(out ppAsmCache, 0));
            return ppAsmCache;
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        [DllImport("clr.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern object GetAssemblyIdentityFromFile([In, MarshalAs(UnmanagedType.LPWStr)] string filePath, [In] ref Guid riid);
        [SecurityCritical]
        private static T GetClrMetaHost<T>()
        {
            return (T) nCLRCreateInstance(_metaHostClsId, _metaHostGuid);
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        [SecurityCritical, DllImport("mscoree.dll", EntryPoint="CLRCreateInstance", PreserveSig=false)]
        private static extern void GetClrMetaHostPolicy(ref Guid clsid, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out IClrMetaHostPolicy ClrMetaHostPolicy);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern uint GetCurrentThreadId();
        [SecurityCritical]
        public static void GetFileVersion(string szFileName, StringBuilder szBuffer, uint cchBuffer, out uint dwLength)
        {
            ((IClrMetaHost) nCLRCreateInstance(_metaHostClsId, _metaHostGuid)).GetVersionFromFile(szFileName, szBuffer, ref cchBuffer);
            dwLength = cchBuffer;
        }

        internal static string GetLoadedModulePath(string moduleName)
        {
            string str = null;
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle != IntPtr.Zero)
            {
                StringBuilder fileName = new StringBuilder(260);
                if (GetModuleFileName(moduleHandle, fileName, fileName.Capacity) > 0)
                {
                    str = fileName.ToString();
                }
            }
            return str;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetModuleFileName(IntPtr module, [Out] StringBuilder fileName, int size);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        public static extern void GetNativeSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO sysInfo);
        [DllImport("mscoree.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        public static extern void GetRequestedRuntimeInfo(string pExe, string pwszVersion, string pConfigurationFile, uint startupFlags, uint runtimeInfoFlags, StringBuilder pDirectory, uint dwDirectory, out uint dwDirectoryLength, StringBuilder pVersion, uint cchBuffer, out uint dwLength);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int GetShortPathName(string LongPath, [Out] StringBuilder ShortPath, int BufferSize);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        public static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO sysInfo);
        [DllImport("wininet.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern bool InternetGetCookieW([In] string url, [In] string cookieName, [Out] StringBuilder cookieData, [In, Out] ref uint bytes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr LoadLibraryEx(string lpModuleName, IntPtr hFile, uint dwFlags);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr handle);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr LockResource(IntPtr hglobal);
        [return: MarshalAs(UnmanagedType.Interface)]
        [SecurityCritical, DllImport("mscoree.dll", EntryPoint="CLRCreateInstance", PreserveSig=false)]
        private static extern object nCLRCreateInstance([MarshalAs(UnmanagedType.LPStruct)] Guid clsid, [MarshalAs(UnmanagedType.LPStruct)] Guid iid);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern void ReleaseActCtx([In] IntPtr hActCtx);
        [DllImport("shell32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern void SHChangeNotify(int eventID, uint flags, IntPtr item1, IntPtr item2);
        [DllImport("shell32.dll", CharSet=CharSet.Unicode)]
        public static extern uint SHCreateItemFromParsingName([In, MarshalAs(UnmanagedType.LPWStr)] string pszPath, [In] IntPtr pbc, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr handle);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool VerifyVersionInfo([In, Out] OSVersionInfoEx osvi, [In] uint dwTypeMask, [In] ulong dwConditionMask);
        [DllImport("kernel32.dll")]
        public static extern ulong VerSetConditionMask([In] ulong ConditionMask, [In] uint TypeMask, [In] byte Condition);

        [StructLayout(LayoutKind.Explicit)]
        public struct _PROCESSOR_INFO_UNION
        {
            [FieldOffset(0)]
            internal uint dwOemId;
            [FieldOffset(0)]
            internal ushort wProcessorArchitecture;
            [FieldOffset(2)]
            internal ushort wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class ACTCTXW
        {
            public uint cbSize = ((uint) Marshal.SizeOf(typeof(System.Deployment.Application.NativeMethods.ACTCTXW)));
            public uint dwFlags = 0;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpSource;
            public ushort wProcessorArchitecture;
            public ushort wLangId;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpAssemblyDirectory;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpResourceName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpApplicationName;
            public IntPtr hModule;
            public ACTCTXW(string manifestPath)
            {
                this.lpSource = manifestPath;
            }
        }

        internal enum ASM_CACHE : uint
        {
            DOWNLOAD = 4,
            GAC = 2,
            ZAP = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AssemblyInfoInternal
        {
            internal const int MaxPath = 0x400;
            internal int cbAssemblyInfo;
            internal int assemblyFlags;
            internal long assemblySizeInKB;
            internal IntPtr currentAssemblyPathBuf;
            internal int cchBuf;
        }

        public enum CacheEntryFlags : uint
        {
            Cookie = 0x100000,
            Edited = 8,
            Normal = 1,
            Sparse = 0x10000,
            Sticky = 4,
            TrackOffline = 0x10,
            TrackOnline = 0x20,
            UrlHistory = 0x200000
        }

        public sealed class CCorRuntimeHost : IDisposable
        {
            private string ClrRuntimeInfoVersion = string.Empty;
            private Host_CurrentDomain CurrentDomainFnPtr;
            private IntPtr DomainObjectPtr = IntPtr.Zero;
            private IntPtr DomainTypePtr = IntPtr.Zero;
            private bool fDelegatesBound;
            private AppDomain_GetType GetTypeFnPtr;
            private Type_InvokeMember InvokeMemberFnPtr;
            private System.Deployment.Application.NativeMethods.ICorRuntimeHost RuntimeHostInstance;
            private IntPtr RuntimeHostPtr = IntPtr.Zero;

            public CCorRuntimeHost(System.Deployment.Application.NativeMethods.IClrRuntimeInfo RuntimeInfo)
            {
                StringBuilder buffer = new StringBuilder(260);
                int capacity = buffer.Capacity;
                RuntimeInfo.GetVersionString(buffer, ref capacity);
                this.ClrRuntimeInfoVersion = buffer.ToString();
                Logger.AddMethodCall("CCorRuntimeHost.ctor called with IClrRuntimeInfo version " + this.ClrRuntimeInfoVersion, DateTime.Now);
                this.RuntimeHostInstance = (System.Deployment.Application.NativeMethods.ICorRuntimeHost) RuntimeInfo.GetInterface(System.Deployment.Application.NativeMethods._corRuntimeHostClsIdGuid, System.Deployment.Application.NativeMethods._corRuntimeHostInterfaceIdGuid);
            }

            public string ApplyPolicyInOtherRuntime(string name)
            {
                if (!this.fDelegatesBound)
                {
                    this.BindDelegatesToManualCOMPInvokeFunctionPointers();
                }
                object[] args = new object[] { name };
                object retval = null;
                VARIANT target = new VARIANT {
                    vt = 13,
                    data1 = this.DomainObjectPtr
                };
                int errorCode = this.InvokeMemberFnPtr(this.DomainTypePtr, "ApplyPolicy", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, target, args, out retval);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return retval.ToString();
            }

            private void BindDelegatesToManualCOMPInvokeFunctionPointers()
            {
                if (!this.fDelegatesBound)
                {
                    IntPtr ptr;
                    this.RuntimeHostInstance.Start();
                    int ofs = 0x15 * IntPtr.Size;
                    int num2 = 10 * IntPtr.Size;
                    int num3 = 0x39 * IntPtr.Size;
                    this.RuntimeHostPtr = Marshal.GetIUnknownForObject(this.RuntimeHostInstance);
                    this.CurrentDomainFnPtr = (Host_CurrentDomain) Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(this.RuntimeHostPtr), ofs), typeof(Host_CurrentDomain));
                    int errorCode = this.CurrentDomainFnPtr(this.RuntimeHostPtr, out ptr);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    Guid gUID = typeof(_AppDomain).GUID;
                    errorCode = Marshal.QueryInterface(ptr, ref gUID, out this.DomainObjectPtr);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    this.GetTypeFnPtr = (AppDomain_GetType) Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(this.DomainObjectPtr), num2), typeof(AppDomain_GetType));
                    errorCode = this.GetTypeFnPtr(this.DomainObjectPtr, out this.DomainTypePtr);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    this.InvokeMemberFnPtr = (Type_InvokeMember) Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(this.DomainTypePtr), num3), typeof(Type_InvokeMember));
                    this.fDelegatesBound = true;
                }
            }

            public void Dispose()
            {
                this.fDelegatesBound = false;
                this.InvokeMemberFnPtr = null;
                if (IntPtr.Zero != this.DomainTypePtr)
                {
                    Marshal.Release(this.DomainTypePtr);
                    this.DomainTypePtr = IntPtr.Zero;
                }
                this.GetTypeFnPtr = null;
                if (IntPtr.Zero != this.DomainObjectPtr)
                {
                    Marshal.Release(this.DomainObjectPtr);
                    this.DomainObjectPtr = IntPtr.Zero;
                }
                this.CurrentDomainFnPtr = null;
                if (IntPtr.Zero != this.RuntimeHostPtr)
                {
                    Marshal.Release(this.RuntimeHostPtr);
                    this.RuntimeHostPtr = IntPtr.Zero;
                }
                this.RuntimeHostInstance = null;
            }

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            private delegate int AppDomain_GetType(IntPtr _this, out IntPtr domainType);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            private delegate int Host_CurrentDomain(IntPtr _this, out IntPtr domain);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            private delegate int Type_InvokeMember(IntPtr _this, [MarshalAs(UnmanagedType.BStr)] string name, BindingFlags invokeAttr, Binder binder, System.Deployment.Application.NativeMethods.CCorRuntimeHost.VARIANT target, [MarshalAs(UnmanagedType.SafeArray)] object[] args, out object retval);

            [StructLayout(LayoutKind.Sequential)]
            private struct VARIANT
            {
                public const ushort VT_UNKNOWN = 13;
                public ushort vt;
                public ushort wReserved1;
                public ushort wReserved2;
                public ushort wReserved3;
                public IntPtr data1;
                public IntPtr data2;
            }
        }

        internal delegate int CoInitializeEEDelegate(uint fFlags);

        internal delegate int CreateAssemblyCacheDelegate([MarshalAs(UnmanagedType.Interface)] out System.Deployment.Application.NativeMethods.IAssemblyCache ppAsmCache, uint reserved);

        internal enum CreateAssemblyNameObjectFlags : uint
        {
            CANOF_DEFAULT = 0,
            CANOF_PARSE_DISPLAY_NAME = 1
        }

        internal enum CreationDisposition : uint
        {
            CREATE_ALWAYS = 2,
            CREATE_NEW = 1,
            OPEN_ALWAYS = 4,
            OPEN_EXISTING = 3,
            TRUNCATE_EXISTING = 5
        }

        [Flags]
        internal enum FlagsAndAttributes : uint
        {
            FILE_ATTRIBUTE_ARCHIVE = 0x20,
            FILE_ATTRIBUTE_COMPRESSED = 0x800,
            FILE_ATTRIBUTE_DEVICE = 0x40,
            FILE_ATTRIBUTE_DIRECTORY = 0x10,
            FILE_ATTRIBUTE_ENCRYPTED = 0x4000,
            FILE_ATTRIBUTE_HIDDEN = 2,
            FILE_ATTRIBUTE_NORMAL = 0x80,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000,
            FILE_ATTRIBUTE_OFFLINE = 0x1000,
            FILE_ATTRIBUTE_READONLY = 1,
            FILE_ATTRIBUTE_REPARSE_POINT = 0x400,
            FILE_ATTRIBUTE_SPARSE_FILE = 0x200,
            FILE_ATTRIBUTE_SYSTEM = 4,
            FILE_ATTRIBUTE_TEMPORARY = 0x100,
            FILE_FLAG_BACKUP_SEMANTICS = 0x2000000,
            FILE_FLAG_DELETE_ON_CLOSE = 0x4000000,
            FILE_FLAG_FIRST_PIPE_INSTANCE = 0x80000,
            FILE_FLAG_NO_BUFFERING = 0x20000000,
            FILE_FLAG_OPEN_NO_RECALL = 0x100000,
            FILE_FLAG_OPEN_REPARSE_POINT = 0x200000,
            FILE_FLAG_OVERLAPPED = 0x40000000,
            FILE_FLAG_POSIX_SEMANTICS = 0x1000000,
            FILE_FLAG_RANDOM_ACCESS = 0x10000000,
            FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000,
            FILE_FLAG_WRITE_THROUGH = 0x80000000
        }

        [Flags]
        internal enum GenericAccess : uint
        {
            GENERIC_ALL = 0x10000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        internal enum HResults
        {
            HRESULT_ERROR_REVISION_MISMATCH = -2147023590
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7c23ff90-33af-11d3-95da-00a024a85b51")]
        internal interface IApplicationContext
        {
            void SetContextNameObject(System.Deployment.Application.NativeMethods.IAssemblyName pName);
            void GetContextNameObject(out System.Deployment.Application.NativeMethods.IAssemblyName ppName);
            void Set([MarshalAs(UnmanagedType.LPWStr)] string szName, int pvValue, uint cbValue, uint dwFlags);
            void Get([MarshalAs(UnmanagedType.LPWStr)] string szName, out int pvValue, ref uint pcbValue, uint dwFlags);
            void GetDynamicDirectory(out int wzDynamicDir, ref uint pdwSize);
        }

        [ComImport, Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAssemblyCache
        {
            void UninstallAssembly();
            void QueryAssemblyInfo(int flags, [MarshalAs(UnmanagedType.LPWStr)] string assemblyName, ref System.Deployment.Application.NativeMethods.AssemblyInfoInternal assemblyInfo);
            void CreateAssemblyCacheItem();
            void CreateAssemblyScavenger();
            void InstallAssembly();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
        internal interface IAssemblyEnum
        {
            [PreserveSig]
            int GetNextAssembly(System.Deployment.Application.NativeMethods.IApplicationContext ppAppCtx, out System.Deployment.Application.NativeMethods.IAssemblyName ppName, uint dwFlags);
            [PreserveSig]
            int Reset();
            [PreserveSig]
            int Clone(out System.Deployment.Application.NativeMethods.IAssemblyEnum ppEnum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
        internal interface IAssemblyName
        {
            [PreserveSig]
            int SetProperty(uint PropertyId, IntPtr pvProperty, uint cbProperty);
            [PreserveSig]
            int GetProperty(uint PropertyId, IntPtr pvProperty, ref uint pcbProperty);
            [PreserveSig]
            int Finalize();
            [PreserveSig]
            int GetDisplayName(IntPtr szDisplayName, ref uint pccDisplayName, uint dwDisplayFlags);
            [PreserveSig]
            int BindToObject(object refIID, object pAsmBindSink, System.Deployment.Application.NativeMethods.IApplicationContext pApplicationContext, [MarshalAs(UnmanagedType.LPWStr)] string szCodeBase, long llFlags, int pvReserved, uint cbReserved, out int ppv);
            [PreserveSig]
            int GetName(out uint lpcwBuffer, out int pwzName);
            [PreserveSig]
            int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);
            [PreserveSig]
            int IsEqual(System.Deployment.Application.NativeMethods.IAssemblyName pName, uint dwCmpFlags);
            [PreserveSig]
            int Clone(out System.Deployment.Application.NativeMethods.IAssemblyName pName);
        }

        [ComImport, SecurityCritical, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("D332DB9E-B9B3-4125-8207-A14884F53216")]
        public interface IClrMetaHost
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            object GetRuntime([In, MarshalAs(UnmanagedType.LPWStr)] string version, [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            void GetVersionFromFile([In, MarshalAs(UnmanagedType.LPWStr)] string filePath, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In, Out, MarshalAs(UnmanagedType.U4)] ref uint bufferLength);
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            System.Deployment.Application.NativeMethods.IEnumUnknown EnumerateInstalledRuntimes();
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            System.Deployment.Application.NativeMethods.IEnumUnknown EnumerateLoadedRuntimes([In] IntPtr processHandle);
            [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            int Reserved01([In] IntPtr reserved1);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("E2190695-77B2-492E-8E14-C4B3A7FDD593"), SecurityCritical]
        public interface IClrMetaHostPolicy
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            object GetRequestedRuntime([In, ComAliasName("Microsoft.Runtime.Hosting.Interop.MetaHostPolicyFlags")] System.Deployment.Application.NativeMethods.MetaHostPolicyFlags policyFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string binaryPath, [In, MarshalAs(UnmanagedType.Interface)] IStream configStream, [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder version, [In, Out, MarshalAs(UnmanagedType.U4)] ref int versionLength, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder imageVersion, [In, Out, MarshalAs(UnmanagedType.U4)] ref int imageVersionLength, [MarshalAs(UnmanagedType.U4)] out int pdwConfigFlags, [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);
        }

        [ComImport, SecurityCritical, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
        public interface IClrRuntimeInfo
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            void GetVersionString([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            void GetRuntimeDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);
            [return: MarshalAs(UnmanagedType.Bool)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            bool IsLoaded([In] IntPtr processHandle);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), LCIDConversion(3)]
            void LoadErrorString([In, MarshalAs(UnmanagedType.U4)] int resourceId, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In, Out, MarshalAs(UnmanagedType.U4)] ref int bufferLength);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            IntPtr LoadLibrary([In, MarshalAs(UnmanagedType.LPWStr)] string dllName);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            IntPtr GetProcAddress([In, MarshalAs(UnmanagedType.LPStr)] string procName);
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            object GetInterface([In, MarshalAs(UnmanagedType.LPStruct)] Guid coClassId, [In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);
        }

        [ComImport, Guid("CB2F6722-AB3A-11d2-9C40-00C04FA30A3E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICorRuntimeHost
        {
            [PreserveSig]
            int CreateLogicalThreadState();
            [PreserveSig]
            int DeleteLogicalThreadState();
            [PreserveSig]
            int SwitchInLogicalThreadState([In] ref uint pFiberCookie);
            [PreserveSig]
            int SwitchOutLogicalThreadState(out uint FiberCookie);
            [PreserveSig]
            int LocksHeldByLogicalThread(out uint pCount);
            [PreserveSig]
            int MapFile(IntPtr hFile, out IntPtr hMapAddress);
            [PreserveSig]
            int GetConfiguration([MarshalAs(UnmanagedType.IUnknown)] out object pConfiguration);
            [PreserveSig]
            int Start();
            [PreserveSig]
            int Stop();
            [PreserveSig]
            int CreateDomain(string pwzFriendlyName, [MarshalAs(UnmanagedType.IUnknown)] object pIdentityArray, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int GetDefaultDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int EnumDomains(out IntPtr hEnum);
            [PreserveSig]
            int NextDomain(IntPtr hEnum, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int CloseEnum(IntPtr hEnum);
            [PreserveSig]
            int CreateDomainEx(string pwzFriendlyName, [MarshalAs(UnmanagedType.IUnknown)] object pSetup, [MarshalAs(UnmanagedType.IUnknown)] object pEvidence, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int CreateDomainSetup([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomainSetup);
            [PreserveSig]
            int CreateEvidence([MarshalAs(UnmanagedType.IUnknown)] out object pEvidence);
            [PreserveSig]
            int UnloadDomain([MarshalAs(UnmanagedType.IUnknown)] object pAppDomain);
            [PreserveSig]
            int CurrentDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
        }

        [ComImport, SecurityCritical, Guid("00000100-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEnumUnknown
        {
            [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            int Next([In, MarshalAs(UnmanagedType.U4)] int elementArrayLength, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.IUnknown, SizeParamIndex=0)] object[] elementArray, [MarshalAs(UnmanagedType.U4)] out int fetchedElementCount);
            [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int count);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            void Reset();
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
            void Clone([MarshalAs(UnmanagedType.Interface)] out System.Deployment.Application.NativeMethods.IEnumUnknown enumerator);
        }

        [ComImport, Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItem
        {
            void BindToHandler(IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppv);
            void GetParent(out System.Deployment.Application.NativeMethods.IShellItem ppsi);
            void GetDisplayName(System.Deployment.Application.NativeMethods.SIGDN sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(System.Deployment.Application.NativeMethods.IShellItem psi, uint hint, out int piOrder);
        }

        [ComImport, Guid("4CD19ADA-25A5-4A32-B3B7-347BEE5BE36B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IStartMenuPinnedList
        {
            void RemoveFromList(System.Deployment.Application.NativeMethods.IShellItem psi);
        }

        [Flags, SecurityCritical]
        public enum MetaHostPolicyFlags
        {
            MetaHostPolicyApplyUpgradePolicy = 8,
            MetaHostPolicyEmulateExeLaunch = 15,
            MetaHostPolicyHighCompatibility = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public class OSVersionInfoEx
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
            public string szCSDVersion;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte bProductType;
            public byte bReserved;
        }

        [StructLayout(LayoutKind.Sequential), SuppressUnmanagedCodeSecurity]
        internal class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
            ~PROCESS_INFORMATION()
            {
                this.Close();
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void Close()
            {
                if ((this.hProcess != IntPtr.Zero) && (this.hProcess != System.Deployment.Application.NativeMethods.INVALID_HANDLE_VALUE))
                {
                    System.Deployment.Application.NativeMethods.CloseHandle(new HandleRef(this, this.hProcess));
                    this.hProcess = System.Deployment.Application.NativeMethods.INVALID_HANDLE_VALUE;
                }
                if ((this.hThread != IntPtr.Zero) && (this.hThread != System.Deployment.Application.NativeMethods.INVALID_HANDLE_VALUE))
                {
                    System.Deployment.Application.NativeMethods.CloseHandle(new HandleRef(this, this.hThread));
                    this.hThread = System.Deployment.Application.NativeMethods.INVALID_HANDLE_VALUE;
                }
            }
        }

        [Flags]
        internal enum ShareMode : uint
        {
            FILE_SHARE_DELETE = 4,
            FILE_SHARE_NONE = 0,
            FILE_SHARE_READ = 1,
            FILE_SHARE_WRITE = 2
        }

        public enum SHChangeNotifyEventID
        {
            SHCNE_ASSOCCHANGED = 0x8000000
        }

        public enum SHChangeNotifyFlags : uint
        {
            SHCNF_IDLIST = 0
        }

        internal enum SIGDN : uint
        {
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            FILESYSPATH = 0x80058000,
            NORMALDISPLAY = 0,
            PARENTRELATIVE = 0x80080001,
            PARENTRELATIVEEDITING = 0x80031001,
            PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            PARENTRELATIVEPARSING = 0x80018001,
            URL = 0x80068000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            internal System.Deployment.Application.NativeMethods._PROCESSOR_INFO_UNION uProcessorInfo;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint dwProcessorLevel;
            public uint dwProcessorRevision;
        }

        public enum tagCOINITEE : uint
        {
            COINITEE_DEFAULT = 0,
            COINITEE_DLL = 1,
            COINITEE_MAIN = 2
        }

        internal enum Win32Error
        {
            ERROR_ACCESS_DENIED = 5,
            ERROR_ALREADY_EXISTS = 0xb7,
            ERROR_CALL_NOT_IMPLEMENTED = 120,
            ERROR_FILE_EXISTS = 80,
            ERROR_FILE_NOT_FOUND = 2,
            ERROR_FILENAME_EXCED_RANGE = 0xce,
            ERROR_INVALID_FUNCTION = 1,
            ERROR_INVALID_HANDLE = 6,
            ERROR_INVALID_PARAMETER = 0x57,
            ERROR_NO_MORE_FILES = 0x12,
            ERROR_NOT_READY = 0x15,
            ERROR_PATH_NOT_FOUND = 3,
            ERROR_SHARING_VIOLATION = 0x20,
            ERROR_SUCCESS = 0,
            ERROR_TOO_MANY_OPEN_FILES = 4
        }
    }
}

