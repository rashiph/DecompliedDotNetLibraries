namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    internal static class NativeMethods
    {
        private const string Advapi32DLL = "advapi32.dll";
        internal const uint COMIMAGE_FLAGS_STRONGNAMESIGNED = 8;
        private const string Crypt32DLL = "crypt32.dll";
        internal const uint FILE_MAP_READ = 4;
        internal const uint FILE_TYPE_DISK = 1;
        internal const uint GENERIC_READ = 0x80000000;
        internal static Guid GUID_ExportedFromComPlus = new Guid("{90883f05-3d28-11d2-8f17-00a0c9a6186d}");
        internal static Guid GUID_TYPELIB_NAMESPACE = new Guid("{0F21F359-AB84-41E8-9A78-36D110E6D2F9}");
        internal const int HRESULT_E_CLASSNOTREGISTERED = -2147221164;
        internal static Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");
        internal static Guid IID_IDispatchEx = new Guid("{A6EF9860-C720-11D0-9337-00A0C90DCAA9}");
        internal static Guid IID_IEnumVariant = new Guid("{00020404-0000-0000-C000-000000000046}");
        internal static Guid IID_ITypeInfo = new Guid("{00020401-0000-0000-C000-000000000046}");
        internal static Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        internal static Guid IID_StdOle = new Guid("{00020430-0000-0000-C000-000000000046}");
        internal const uint IMAGE_DIRECTORY_ENTRY_COMHEADER = 14;
        internal const uint IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b;
        internal const uint IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;
        internal static IntPtr InvalidIntPtr = new IntPtr(-1);
        private const string MscoreeDLL = "mscoree.dll";
        internal const uint PAGE_READONLY = 2;
        internal const int SE_ERR_ACCESSDENIED = 5;
        internal const int TYPE_E_CANTLOADLIBRARY = -2147312566;
        internal const int TYPE_E_REGISTRYACCESS = -2147319780;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("crypt32.dll", SetLastError=true)]
        internal static extern bool CertCloseStore([In] IntPtr CertStore, CertStoreClose Flags);
        [DllImport("crypt32.dll", SetLastError=true)]
        internal static extern IntPtr CertEnumCertificatesInStore([In] IntPtr CertStore, [In] IntPtr PrevCertContext);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("crypt32.dll", SetLastError=true)]
        internal static extern bool CertFreeCertificateContext(IntPtr CertContext);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool CloseHandle(IntPtr hObject);
        [DllImport("fusion.dll", CharSet=CharSet.Unicode)]
        internal static extern int CompareAssemblyIdentity(string pwzAssemblyIdentity1, [MarshalAs(UnmanagedType.Bool)] bool fUnified1, string pwzAssemblyIdentity2, [MarshalAs(UnmanagedType.Bool)] bool fUnified2, [MarshalAs(UnmanagedType.Bool)] out bool pfEquivalent, out AssemblyComparisonResult pResult);
        [DllImport("fusion.dll")]
        internal static extern uint CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);
        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, IntPtr pUnkReserved, IAssemblyName pName, AssemblyCacheFlags flags, IntPtr pvReserved);
        [DllImport("fusion.dll")]
        internal static extern int CreateAssemblyNameObject(out IAssemblyName ppAssemblyNameObj, [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, CreateAssemblyNameObjectFlags flags, IntPtr pvReserved);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CreateHardLink(string newFileName, string exitingFileName, IntPtr securityAttributes);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("crypt32.dll", SetLastError=true)]
        internal static extern bool CryptAcquireCertificatePrivateKey([In] IntPtr CertContext, [In] uint flags, [In] IntPtr reserved, [In, Out] ref IntPtr CryptProv, [In, Out] ref KeySpec KeySpec, [In, Out, MarshalAs(UnmanagedType.Bool)] ref bool CallerFreeProv);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool CryptDestroyKey(IntPtr hKey);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool CryptExportKey([In] IntPtr Key, [In] IntPtr ExpKey, [In] BlobType type, [In] uint Flags, [In] IntPtr Data, [In, Out] ref uint DataLen);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool CryptGetUserKey([In] IntPtr CryptProv, [In] KeySpec KeySpec, [In, Out] ref IntPtr Key);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool CryptReleaseContext([In] IntPtr Prov, [In] uint Flags);
        [DllImport("fusion.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetCachePath(AssemblyCacheFlags cacheFlags, StringBuilder cachePath, ref int pcchPath);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern uint GetFileType(IntPtr hFile);
        [DllImport("mscoree.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern uint GetFileVersion(string szFullPath, StringBuilder szBuffer, int cchBuffer, out uint dwLength);
        [DllImport("dbghelp.dll", SetLastError=true)]
        internal static extern IntPtr ImageNtHeader(IntPtr imageBase);
        [DllImport("dbghelp.dll", SetLastError=true)]
        internal static extern IntPtr ImageRvaToVa(IntPtr ntHeaders, IntPtr imageBase, uint Rva, out IntPtr LastRvaSection);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("oleaut32", PreserveSig=false)]
        internal static extern object LoadRegTypeLib([In] ref Guid clsid, [In] short majorVersion, [In] short minorVersion, [In] int lcid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("oleaut32", PreserveSig=false)]
        internal static extern object LoadTypeLibEx([In, MarshalAs(UnmanagedType.LPWStr)] string szFullPath, [In] int regKind);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMapping, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool MoveFileEx([In] string existingFileName, [In] string newFileName, [In] MoveFileFlags flags);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern IntPtr PFXImportCertStore([In] IntPtr blob, [In] string password, [In] CryptFlags flags);
        [return: MarshalAs(UnmanagedType.BStr)]
        [DllImport("oleaut32", PreserveSig=false)]
        internal static extern string QueryPathOfRegTypeLib([In] ref Guid clsid, [In] short majorVersion, [In] short minorVersion, [In] int lcid);
        [DllImport("oleaut32", PreserveSig=false)]
        internal static extern void RegisterTypeLib([In, MarshalAs(UnmanagedType.Interface)] object pTypeLib, [In, MarshalAs(UnmanagedType.LPWStr)] string szFullPath, [In, MarshalAs(UnmanagedType.LPWStr)] string szHelpDir);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
        [DllImport("oleaut32", EntryPoint="UnRegisterTypeLib", PreserveSig=false)]
        internal static extern void UnregisterTypeLib([In] ref Guid guid, [In] short wMajorVerNum, [In] short wMinorVerNum, [In] int lcid, [In] System.Runtime.InteropServices.ComTypes.SYSKIND syskind);

        [ComVisible(false)]
        internal class AssemblyCacheEnum : IEnumerable<AssemblyNameExtension>, System.Collections.IEnumerable
        {
            private IAssemblyEnum assemblyEnum;
            private bool done;

            internal AssemblyCacheEnum(string assemblyName)
            {
                this.InitializeEnum(assemblyName);
            }

            public IEnumerator<AssemblyNameExtension> GetEnumerator()
            {
                int errorCode = 0;
                IAssemblyName ppName = null;
                if ((this.assemblyEnum == null) || this.done)
                {
                    goto Label_00E5;
                }
            Label_PostSwitchInIterator:;
                if (!this.done)
                {
                    errorCode = this.assemblyEnum.GetNextAssembly(IntPtr.Zero, out ppName, 0);
                    if (errorCode < 0)
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                    if (ppName != null)
                    {
                        string fullName = GetFullName(ppName);
                        yield return new AssemblyNameExtension(fullName);
                        goto Label_PostSwitchInIterator;
                    }
                    this.done = true;
                }
            Label_00E5:;
            }

            private static string GetFullName(IAssemblyName fusionAsmName)
            {
                int capacity = 0x400;
                StringBuilder pDisplayName = new StringBuilder(capacity);
                int errorCode = fusionAsmName.GetDisplayName(pDisplayName, ref capacity, 0xa7);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return pDisplayName.ToString();
            }

            private void InitializeEnum(string assemblyName)
            {
                IAssemblyName ppAssemblyNameObj = null;
                int num = 0;
                if (assemblyName != null)
                {
                    num = Microsoft.Build.Tasks.NativeMethods.CreateAssemblyNameObject(out ppAssemblyNameObj, assemblyName, CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero);
                }
                if (num >= 0)
                {
                    num = Microsoft.Build.Tasks.NativeMethods.CreateAssemblyEnum(out this.assemblyEnum, IntPtr.Zero, ppAssemblyNameObj, AssemblyCacheFlags.GAC, IntPtr.Zero);
                }
                if (num < 0)
                {
                    this.assemblyEnum = null;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

        }

        internal enum AssemblyComparisonResult
        {
            ACR_Unknown,
            ACR_EquivalentFullMatch,
            ACR_EquivalentWeakNamed,
            ACR_EquivalentFXUnified,
            ACR_EquivalentUnified,
            ACR_NonEquivalentVersion,
            ACR_NonEquivalent,
            ACR_EquivalentPartialMatch,
            ACR_EquivalentPartialWeakNamed,
            ACR_EquivalentPartialUnified,
            ACR_EquivalentPartialFXUnified,
            ACR_NonEquivalentPartialVersion
        }

        internal enum BlobType
        {
            OPAQUEKEYBLOB = 9,
            PLAINTEXTKEYBLOB = 8,
            PRIVATEKEYBLOB = 7,
            PUBLICKEYBLOB = 6,
            PUBLICKEYBLOBEX = 10,
            SIMPLEBLOB = 1,
            SYMMETRICWRAPKEYBLOB = 11
        }

        [Flags]
        internal enum CertStoreClose
        {
            CERT_CLOSE_STORE_CHECK_FLAG = 2,
            CERT_CLOSE_STORE_FORCE_FLAG = 1
        }

        [Flags]
        internal enum CryptFlags
        {
            Exportable = 1,
            MachineKeySet = 0x20,
            UserKeySet = 0x1000,
            UserProtected = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPTOAPI_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_COR20_HEADER
        {
            internal uint cb;
            internal ushort MajorRuntimeVersion;
            internal ushort MinorRuntimeVersion;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY MetaData;
            internal uint Flags;
            internal uint EntryPointTokenOrEntryPointRVA;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY Resources;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY StrongNameSignature;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY CodeManagerTable;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY VTableFixups;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_DATA_DIRECTORY ManagedNativeHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_DATA_DIRECTORY
        {
            internal uint VirtualAddress;
            internal uint Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_FILE_HEADER
        {
            internal ushort Machine;
            internal ushort NumberOfSections;
            internal uint TimeDateStamp;
            internal uint PointerToSymbolTable;
            internal uint NumberOfSymbols;
            internal ushort SizeOfOptionalHeader;
            internal ushort Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_NT_HEADERS32
        {
            internal uint signature;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_FILE_HEADER fileHeader;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_OPTIONAL_HEADER32 optionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_NT_HEADERS64
        {
            internal uint signature;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_FILE_HEADER fileHeader;
            internal Microsoft.Build.Tasks.NativeMethods.IMAGE_OPTIONAL_HEADER64 optionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_OPTIONAL_HEADER32
        {
            internal ushort Magic;
            internal byte MajorLinkerVersion;
            internal byte MinorLinkerVersion;
            internal uint SizeOfCode;
            internal uint SizeOfInitializedData;
            internal uint SizeOfUninitializedData;
            internal uint AddressOfEntryPoint;
            internal uint BaseOfCode;
            internal uint BaseOfData;
            internal uint ImageBase;
            internal uint SectionAlignment;
            internal uint FileAlignment;
            internal ushort MajorOperatingSystemVersion;
            internal ushort MinorOperatingSystemVersion;
            internal ushort MajorImageVersion;
            internal ushort MinorImageVersion;
            internal ushort MajorSubsystemVersion;
            internal ushort MinorSubsystemVersion;
            internal uint Win32VersionValue;
            internal uint SizeOfImage;
            internal uint SizeOfHeaders;
            internal uint CheckSum;
            internal ushort Subsystem;
            internal ushort DllCharacteristics;
            internal uint SizeOfStackReserve;
            internal uint SizeOfStackCommit;
            internal uint SizeOfHeapReserve;
            internal uint SizeOfHeapCommit;
            internal uint LoaderFlags;
            internal uint NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            internal ulong[] DataDirectory;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_OPTIONAL_HEADER64
        {
            internal ushort Magic;
            internal byte MajorLinkerVersion;
            internal byte MinorLinkerVersion;
            internal uint SizeOfCode;
            internal uint SizeOfInitializedData;
            internal uint SizeOfUninitializedData;
            internal uint AddressOfEntryPoint;
            internal uint BaseOfCode;
            internal ulong ImageBase;
            internal uint SectionAlignment;
            internal uint FileAlignment;
            internal ushort MajorOperatingSystemVersion;
            internal ushort MinorOperatingSystemVersion;
            internal ushort MajorImageVersion;
            internal ushort MinorImageVersion;
            internal ushort MajorSubsystemVersion;
            internal ushort MinorSubsystemVersion;
            internal uint Win32VersionValue;
            internal uint SizeOfImage;
            internal uint SizeOfHeaders;
            internal uint CheckSum;
            internal ushort Subsystem;
            internal ushort DllCharacteristics;
            internal ulong SizeOfStackReserve;
            internal ulong SizeOfStackCommit;
            internal ulong SizeOfHeapReserve;
            internal ulong SizeOfHeapCommit;
            internal uint LoaderFlags;
            internal uint NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
            internal ulong[] DataDirectory;
        }

        internal enum KeySpec
        {
            AT_KEYEXCHANGE = 1,
            AT_SIGNATURE = 2
        }

        [Flags]
        internal enum MoveFileFlags
        {
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_CREATE_HARDLINK = 0x10,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x20,
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_WRITE_THROUGH = 8
        }

        internal enum REGKIND
        {
            REGKIND_DEFAULT = 0,
            REGKIND_LOAD_TLB_AS_32BIT = 0x20,
            REGKIND_LOAD_TLB_AS_64BIT = 0x40,
            REGKIND_NONE = 2,
            REGKIND_REGISTER = 1
        }
    }
}

