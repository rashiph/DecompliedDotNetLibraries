namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class IsolationInterop
    {
        private static System.Deployment.Internal.Isolation.IAppIdAuthority _appIdAuth = null;
        private static System.Deployment.Internal.Isolation.IIdentityAuthority _idAuth = null;
        private static object _synchObject = new object();
        private static System.Deployment.Internal.Isolation.Store _systemStore = null;
        private static System.Deployment.Internal.Isolation.Store _userStore = null;
        public static Guid GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
        public static Guid IID_ICMS = GetGuidOfType(typeof(ICMS));
        public static Guid IID_IDefinitionIdentity = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IDefinitionIdentity));
        public static Guid IID_IEnumSTORE_ASSEMBLY = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY));
        public static Guid IID_IEnumSTORE_ASSEMBLY_FILE = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_FILE));
        public static Guid IID_IEnumSTORE_CATEGORY = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY));
        public static Guid IID_IEnumSTORE_CATEGORY_INSTANCE = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY_INSTANCE));
        public static Guid IID_IEnumSTORE_DEPLOYMENT_METADATA = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_DEPLOYMENT_METADATA));
        public static Guid IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY));
        public static Guid IID_IManifestInformation = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IManifestInformation));
        public static Guid IID_IStore = GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IStore));
        public const string IsolationDllName = "clr.dll";
        public static Guid SXS_INSTALL_REFERENCE_SCHEME_SXS_STRONGNAME_SIGNED_PRIVATE_ASSEMBLY = new Guid("3ab20ac0-67e8-4512-8385-a487e35df3da");

        [SecuritySafeCritical]
        internal static System.Deployment.Internal.Isolation.IActContext CreateActContext(System.Deployment.Internal.Isolation.IDefinitionAppId AppId)
        {
            CreateActContextParameters parameters;
            CreateActContextParametersSource source;
            CreateActContextParametersSourceDefinitionAppid appid;
            System.Deployment.Internal.Isolation.IActContext context;
            parameters.Size = (uint) Marshal.SizeOf(typeof(CreateActContextParameters));
            parameters.Flags = 0x10;
            parameters.CustomStoreList = IntPtr.Zero;
            parameters.CultureFallbackList = IntPtr.Zero;
            parameters.ProcessorArchitectureList = IntPtr.Zero;
            parameters.Source = IntPtr.Zero;
            parameters.ProcArch = 0;
            source.Size = (uint) Marshal.SizeOf(typeof(CreateActContextParametersSource));
            source.Flags = 0;
            source.SourceType = 1;
            source.Data = IntPtr.Zero;
            appid.Size = (uint) Marshal.SizeOf(typeof(CreateActContextParametersSourceDefinitionAppid));
            appid.Flags = 0;
            appid.AppId = AppId;
            try
            {
                source.Data = appid.ToIntPtr();
                parameters.Source = source.ToIntPtr();
                context = CreateActContext(ref parameters) as System.Deployment.Internal.Isolation.IActContext;
            }
            finally
            {
                if (source.Data != IntPtr.Zero)
                {
                    CreateActContextParametersSourceDefinitionAppid.Destroy(source.Data);
                    source.Data = IntPtr.Zero;
                }
                if (parameters.Source != IntPtr.Zero)
                {
                    CreateActContextParametersSource.Destroy(parameters.Source);
                    parameters.Source = IntPtr.Zero;
                }
            }
            return context;
        }

        internal static System.Deployment.Internal.Isolation.IActContext CreateActContext(System.Deployment.Internal.Isolation.IReferenceAppId AppId)
        {
            CreateActContextParameters parameters;
            CreateActContextParametersSource source;
            CreateActContextParametersSourceReferenceAppid appid;
            System.Deployment.Internal.Isolation.IActContext context;
            parameters.Size = (uint) Marshal.SizeOf(typeof(CreateActContextParameters));
            parameters.Flags = 0x10;
            parameters.CustomStoreList = IntPtr.Zero;
            parameters.CultureFallbackList = IntPtr.Zero;
            parameters.ProcessorArchitectureList = IntPtr.Zero;
            parameters.Source = IntPtr.Zero;
            parameters.ProcArch = 0;
            source.Size = (uint) Marshal.SizeOf(typeof(CreateActContextParametersSource));
            source.Flags = 0;
            source.SourceType = 2;
            source.Data = IntPtr.Zero;
            appid.Size = (uint) Marshal.SizeOf(typeof(CreateActContextParametersSourceReferenceAppid));
            appid.Flags = 0;
            appid.AppId = AppId;
            try
            {
                source.Data = appid.ToIntPtr();
                parameters.Source = source.ToIntPtr();
                context = CreateActContext(ref parameters) as System.Deployment.Internal.Isolation.IActContext;
            }
            finally
            {
                if (source.Data != IntPtr.Zero)
                {
                    CreateActContextParametersSourceDefinitionAppid.Destroy(source.Data);
                    source.Data = IntPtr.Zero;
                }
                if (parameters.Source != IntPtr.Zero)
                {
                    CreateActContextParametersSource.Destroy(parameters.Source);
                    parameters.Source = IntPtr.Zero;
                }
            }
            return context;
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        [DllImport("clr.dll", PreserveSig=false)]
        internal static extern object CreateActContext(ref CreateActContextParameters Params);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        internal static extern object CreateCMSFromXml([In] byte[] buffer, [In] uint bufferSize, [In] System.Deployment.Internal.Isolation.IManifestParseErrorCallback Callback, [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern System.Deployment.Internal.Isolation.IAppIdAuthority GetAppIdAuthority();
        internal static Guid GetGuidOfType(Type type)
        {
            GuidAttribute attribute = (GuidAttribute) Attribute.GetCustomAttribute(type, typeof(GuidAttribute), false);
            return new Guid(attribute.Value);
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern System.Deployment.Internal.Isolation.IIdentityAuthority GetIdentityAuthority();
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern object GetSystemStore([In] uint Flags, [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [DllImport("clr.dll", PreserveSig=false)]
        internal static extern object GetUserStateManager([In] uint Flags, [In] IntPtr hToken, [In] ref Guid riid);
        [SecuritySafeCritical]
        public static System.Deployment.Internal.Isolation.Store GetUserStore()
        {
            return new System.Deployment.Internal.Isolation.Store(GetUserStore(0, IntPtr.Zero, ref IID_IStore) as System.Deployment.Internal.Isolation.IStore);
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern object GetUserStore([In] uint Flags, [In] IntPtr hToken, [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        internal static extern object ParseManifest([In, MarshalAs(UnmanagedType.LPWStr)] string pszManifestPath, [In] System.Deployment.Internal.Isolation.IManifestParseErrorCallback pIManifestParseErrorCallback, [In] ref Guid riid);

        public static System.Deployment.Internal.Isolation.IAppIdAuthority AppIdAuthority
        {
            [SecuritySafeCritical]
            get
            {
                if (_appIdAuth == null)
                {
                    lock (_synchObject)
                    {
                        if (_appIdAuth == null)
                        {
                            _appIdAuth = GetAppIdAuthority();
                        }
                    }
                }
                return _appIdAuth;
            }
        }

        public static System.Deployment.Internal.Isolation.IIdentityAuthority IdentityAuthority
        {
            [SecuritySafeCritical]
            get
            {
                if (_idAuth == null)
                {
                    lock (_synchObject)
                    {
                        if (_idAuth == null)
                        {
                            _idAuth = GetIdentityAuthority();
                        }
                    }
                }
                return _idAuth;
            }
        }

        public static System.Deployment.Internal.Isolation.Store SystemStore
        {
            get
            {
                if (_systemStore == null)
                {
                    lock (_synchObject)
                    {
                        if (_systemStore == null)
                        {
                            _systemStore = new System.Deployment.Internal.Isolation.Store(GetSystemStore(0, ref IID_IStore) as System.Deployment.Internal.Isolation.IStore);
                        }
                    }
                }
                return _systemStore;
            }
        }

        public static System.Deployment.Internal.Isolation.Store UserStore
        {
            get
            {
                if (_userStore == null)
                {
                    lock (_synchObject)
                    {
                        if (_userStore == null)
                        {
                            _userStore = new System.Deployment.Internal.Isolation.Store(GetUserStore(0, IntPtr.Zero, ref IID_IStore) as System.Deployment.Internal.Isolation.IStore);
                        }
                    }
                }
                return _userStore;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParameters
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint Size;
            [MarshalAs(UnmanagedType.U4)]
            public uint Flags;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr CustomStoreList;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr CultureFallbackList;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr ProcessorArchitectureList;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr Source;
            [MarshalAs(UnmanagedType.U2)]
            public ushort ProcArch;
            [Flags]
            public enum CreateFlags
            {
                CultureListValid = 2,
                IgnoreVisibility = 0x20,
                Nothing = 0,
                ProcessorFallbackListValid = 4,
                ProcessorValid = 8,
                SourceValid = 0x10,
                StoreListValid = 1
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParametersSource
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint Size;
            [MarshalAs(UnmanagedType.U4)]
            public uint Flags;
            [MarshalAs(UnmanagedType.U4)]
            public uint SourceType;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr Data;
            [SecurityCritical]
            public IntPtr ToIntPtr()
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
                Marshal.StructureToPtr(this, ptr, false);
                return ptr;
            }

            [SecurityCritical]
            public static void Destroy(IntPtr p)
            {
                Marshal.DestroyStructure(p, typeof(System.Deployment.Internal.Isolation.IsolationInterop.CreateActContextParametersSource));
                Marshal.FreeCoTaskMem(p);
            }
            [Flags]
            public enum SourceFlags
            {
                Definition = 1,
                Reference = 2
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParametersSourceDefinitionAppid
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint Size;
            [MarshalAs(UnmanagedType.U4)]
            public uint Flags;
            public System.Deployment.Internal.Isolation.IDefinitionAppId AppId;
            [SecurityCritical]
            public IntPtr ToIntPtr()
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
                Marshal.StructureToPtr(this, ptr, false);
                return ptr;
            }

            [SecurityCritical]
            public static void Destroy(IntPtr p)
            {
                Marshal.DestroyStructure(p, typeof(System.Deployment.Internal.Isolation.IsolationInterop.CreateActContextParametersSourceDefinitionAppid));
                Marshal.FreeCoTaskMem(p);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CreateActContextParametersSourceReferenceAppid
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint Size;
            [MarshalAs(UnmanagedType.U4)]
            public uint Flags;
            public System.Deployment.Internal.Isolation.IReferenceAppId AppId;
            public IntPtr ToIntPtr()
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
                Marshal.StructureToPtr(this, ptr, false);
                return ptr;
            }

            public static void Destroy(IntPtr p)
            {
                Marshal.DestroyStructure(p, typeof(System.Deployment.Internal.Isolation.IsolationInterop.CreateActContextParametersSourceReferenceAppid));
                Marshal.FreeCoTaskMem(p);
            }
        }
    }
}

