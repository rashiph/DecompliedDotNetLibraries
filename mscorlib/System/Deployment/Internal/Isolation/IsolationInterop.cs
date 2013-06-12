namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class IsolationInterop
    {
        private static IAppIdAuthority _appIdAuth = null;
        private static IIdentityAuthority _idAuth = null;
        private static object _synchObject = new object();
        public static Guid GUID_SXS_INSTALL_REFERENCE_SCHEME_OPAQUESTRING = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
        public static Guid IID_ICMS = GetGuidOfType(typeof(ICMS));
        public static Guid IID_IDefinitionIdentity = GetGuidOfType(typeof(IDefinitionIdentity));
        public static Guid IID_IEnumSTORE_ASSEMBLY = GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY));
        public static Guid IID_IEnumSTORE_ASSEMBLY_FILE = GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
        public static Guid IID_IEnumSTORE_CATEGORY = GetGuidOfType(typeof(IEnumSTORE_CATEGORY));
        public static Guid IID_IEnumSTORE_CATEGORY_INSTANCE = GetGuidOfType(typeof(IEnumSTORE_CATEGORY_INSTANCE));
        public static Guid IID_IEnumSTORE_DEPLOYMENT_METADATA = GetGuidOfType(typeof(IEnumSTORE_DEPLOYMENT_METADATA));
        public static Guid IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY = GetGuidOfType(typeof(IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY));
        public static Guid IID_IManifestInformation = GetGuidOfType(typeof(IManifestInformation));
        public static Guid IID_IStore = GetGuidOfType(typeof(IStore));
        public const string IsolationDllName = "clr.dll";
        public static Guid SXS_INSTALL_REFERENCE_SCHEME_SXS_STRONGNAME_SIGNED_PRIVATE_ASSEMBLY = new Guid("3ab20ac0-67e8-4512-8385-a487e35df3da");

        [SecuritySafeCritical]
        internal static IActContext CreateActContext(IDefinitionAppId AppId)
        {
            CreateActContextParameters parameters;
            CreateActContextParametersSource source;
            CreateActContextParametersSourceDefinitionAppid appid;
            IActContext context;
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
                context = CreateActContext(ref parameters) as IActContext;
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
        internal static extern object CreateCMSFromXml([In] byte[] buffer, [In] uint bufferSize, [In] IManifestParseErrorCallback Callback, [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern IAppIdAuthority GetAppIdAuthority();
        internal static Guid GetGuidOfType(Type type)
        {
            GuidAttribute attribute = (GuidAttribute) Attribute.GetCustomAttribute(type, typeof(GuidAttribute), false);
            return new Guid(attribute.Value);
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern IIdentityAuthority GetIdentityAuthority();
        [SecuritySafeCritical]
        public static Store GetUserStore()
        {
            return new Store(GetUserStore(0, IntPtr.Zero, ref IID_IStore) as IStore);
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        private static extern object GetUserStore([In] uint Flags, [In] IntPtr hToken, [In] ref Guid riid);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [SecurityCritical, DllImport("clr.dll", PreserveSig=false)]
        internal static extern object ParseManifest([In, MarshalAs(UnmanagedType.LPWStr)] string pszManifestPath, [In] IManifestParseErrorCallback pIManifestParseErrorCallback, [In] ref Guid riid);

        public static IAppIdAuthority AppIdAuthority
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

        public static IIdentityAuthority IdentityAuthority
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
                Marshal.DestroyStructure(p, typeof(IsolationInterop.CreateActContextParametersSource));
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
            public IDefinitionAppId AppId;
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
                Marshal.DestroyStructure(p, typeof(IsolationInterop.CreateActContextParametersSourceDefinitionAppid));
                Marshal.FreeCoTaskMem(p);
            }
        }
    }
}

