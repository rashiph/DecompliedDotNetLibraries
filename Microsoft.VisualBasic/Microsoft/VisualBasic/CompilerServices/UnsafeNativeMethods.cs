namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal sealed class UnsafeNativeMethods
    {
        public const int LCID_US_ENGLISH = 0x409;
        public const int MEMBERID_NIL = 0;

        [SecurityCritical]
        private UnsafeNativeMethods()
        {
        }

        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeFileHandle CreateFileMapping(HandleRef hFile, [MarshalAs(UnmanagedType.LPStruct)] NativeTypes.SECURITY_ATTRIBUTES lpAttributes, int flProtect, int dwMaxSizeHi, int dwMaxSizeLow, string lpName);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetDiskFreeSpaceEx(string Directory, ref long UserSpaceFree, ref long TotalUserSpace, ref long TotalFreeSpace);
        [SecurityCritical, DllImport("User32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern short GetKeyState(int KeyCode);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Unicode)]
        internal static extern int GetLogicalDrives();
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpSrcStr, int cchSrc, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpDestStr, int cchDest);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        internal static extern int LCMapStringA(int Locale, int dwMapFlags, [MarshalAs(UnmanagedType.LPArray)] byte[] lpSrcStr, int cchSrc, [MarshalAs(UnmanagedType.LPArray)] byte[] lpDestStr, int cchDest);
        [SecurityCritical, DllImport("kernel32", SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr LocalFree(IntPtr LocalHandle);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeMemoryMappedViewOfFileHandle MapViewOfFile(IntPtr hFileMapping, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);
        [SecurityCritical, DllImport("user32", CharSet=CharSet.Unicode)]
        internal static extern int MessageBeep(int uType);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern int MoveFile([In, MarshalAs(UnmanagedType.LPTStr)] string lpExistingFileName, [In, MarshalAs(UnmanagedType.LPTStr)] string lpNewFileName);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeFileHandle OpenFileMapping(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);
        [SecurityCritical, DllImport("kernel32", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int SetLocalTime(NativeTypes.SystemTime systime);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool UnmapViewOfFile(IntPtr pvBaseAddress);
        [SecurityCritical, DllImport("oleaut32", CharSet=CharSet.Unicode, PreserveSig=false)]
        internal static extern void VariantChangeType(out object dest, [In] ref object Src, short wFlags, short vt);
        [SecurityCritical, DllImport("oleaut32", CharSet=CharSet.Unicode, PreserveSig=false)]
        internal static extern object VarNumFromParseNum([MarshalAs(UnmanagedType.LPArray)] byte[] numprsPtr, [MarshalAs(UnmanagedType.LPArray)] byte[] DigitArray, int dwVtBits);
        [SecurityCritical, DllImport("oleaut32", CharSet=CharSet.Unicode)]
        internal static extern int VarParseNumFromStr([In, MarshalAs(UnmanagedType.LPWStr)] string str, int lcid, int dwFlags, [MarshalAs(UnmanagedType.LPArray)] byte[] numprsPtr, [MarshalAs(UnmanagedType.LPArray)] byte[] digits);

        [ComImport, EditorBrowsable(EditorBrowsableState.Never), Guid("00020400-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDispatch
        {
            [PreserveSig, Obsolete("Bad signature. Fix and verify signature before use.", true), SecurityCritical]
            int GetTypeInfoCount();
            [PreserveSig, SecurityCritical]
            int GetTypeInfo([In] int index, [In] int lcid, [MarshalAs(UnmanagedType.Interface)] out UnsafeNativeMethods.ITypeInfo pTypeInfo);
            [PreserveSig, SecurityCritical]
            int GetIDsOfNames();
            [PreserveSig, SecurityCritical]
            int Invoke();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), EditorBrowsable(EditorBrowsableState.Never), Guid("B196B283-BAB4-101A-B69C-00AA00341D07")]
        public interface IProvideClassInfo
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [SecurityCritical]
            UnsafeNativeMethods.ITypeInfo GetClassInfo();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020403-0000-0000-C000-000000000046"), EditorBrowsable(EditorBrowsableState.Never)]
        public interface ITypeComp
        {
            [SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            void RemoteBind([In, MarshalAs(UnmanagedType.LPWStr)] string szName, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [In, MarshalAs(UnmanagedType.U2)] short wFlags, [Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeInfo[] ppTInfo, [Out, MarshalAs(UnmanagedType.LPArray)] System.Runtime.InteropServices.ComTypes.DESCKIND[] pDescKind, [Out, MarshalAs(UnmanagedType.LPArray)] System.Runtime.InteropServices.ComTypes.FUNCDESC[] ppFuncDesc, [Out, MarshalAs(UnmanagedType.LPArray)] System.Runtime.InteropServices.ComTypes.VARDESC[] ppVarDesc, [Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeComp[] ppTypeComp, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pDummy);
            [SecurityCritical]
            void RemoteBindType([In, MarshalAs(UnmanagedType.LPWStr)] string szName, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeInfo[] ppTInfo);
        }

        [ComImport, EditorBrowsable(EditorBrowsableState.Never), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020401-0000-0000-C000-000000000046")]
        public interface ITypeInfo
        {
            [PreserveSig, SecurityCritical]
            int GetTypeAttr(out IntPtr pTypeAttr);
            [PreserveSig, SecurityCritical]
            int GetTypeComp(out UnsafeNativeMethods.ITypeComp pTComp);
            [PreserveSig, SecurityCritical]
            int GetFuncDesc([In, MarshalAs(UnmanagedType.U4)] int index, out IntPtr pFuncDesc);
            [PreserveSig, SecurityCritical]
            int GetVarDesc([In, MarshalAs(UnmanagedType.U4)] int index, out IntPtr pVarDesc);
            [PreserveSig, SecurityCritical]
            int GetNames([In] int memid, [Out, MarshalAs(UnmanagedType.LPArray)] string[] rgBstrNames, [In, MarshalAs(UnmanagedType.U4)] int cMaxNames, [MarshalAs(UnmanagedType.U4)] out int cNames);
            [PreserveSig, SecurityCritical, Obsolete("Bad signature, second param type should be Byref. Fix and verify signature before use.", true)]
            int GetRefTypeOfImplType([In, MarshalAs(UnmanagedType.U4)] int index, out int pRefType);
            [PreserveSig, Obsolete("Bad signature, second param type should be Byref. Fix and verify signature before use.", true), SecurityCritical]
            int GetImplTypeFlags([In, MarshalAs(UnmanagedType.U4)] int index, [Out] int pImplTypeFlags);
            [PreserveSig, SecurityCritical]
            int GetIDsOfNames([In] IntPtr rgszNames, [In, MarshalAs(UnmanagedType.U4)] int cNames, out IntPtr pMemId);
            [PreserveSig, SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            int Invoke();
            [PreserveSig, SecurityCritical]
            int GetDocumentation([In] int memid, [MarshalAs(UnmanagedType.BStr)] out string pBstrName, [MarshalAs(UnmanagedType.BStr)] out string pBstrDocString, [MarshalAs(UnmanagedType.U4)] out int pdwHelpContext, [MarshalAs(UnmanagedType.BStr)] out string pBstrHelpFile);
            [PreserveSig, SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            int GetDllEntry([In] int memid, [In] System.Runtime.InteropServices.ComTypes.INVOKEKIND invkind, [Out, MarshalAs(UnmanagedType.BStr)] string pBstrDllName, [Out, MarshalAs(UnmanagedType.BStr)] string pBstrName, [Out, MarshalAs(UnmanagedType.U2)] short pwOrdinal);
            [PreserveSig, SecurityCritical]
            int GetRefTypeInfo([In] IntPtr hreftype, out UnsafeNativeMethods.ITypeInfo pTypeInfo);
            [PreserveSig, SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            int AddressOfMember();
            [PreserveSig, SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            int CreateInstance([In] ref IntPtr pUnkOuter, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] object ppvObj);
            [PreserveSig, SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            int GetMops([In] int memid, [Out, MarshalAs(UnmanagedType.BStr)] string pBstrMops);
            [PreserveSig, SecurityCritical]
            int GetContainingTypeLib([Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeLib[] ppTLib, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pIndex);
            [PreserveSig, SecurityCritical]
            void ReleaseTypeAttr(IntPtr typeAttr);
            [PreserveSig, SecurityCritical]
            void ReleaseFuncDesc(IntPtr funcDesc);
            [PreserveSig, SecurityCritical]
            void ReleaseVarDesc(IntPtr varDesc);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), EditorBrowsable(EditorBrowsableState.Never), Guid("00020402-0000-0000-C000-000000000046")]
        public interface ITypeLib
        {
            [SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            void RemoteGetTypeInfoCount([Out, MarshalAs(UnmanagedType.LPArray)] int[] pcTInfo);
            [SecurityCritical]
            void GetTypeInfo([In, MarshalAs(UnmanagedType.U4)] int index, [Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeInfo[] ppTInfo);
            [SecurityCritical]
            void GetTypeInfoType([In, MarshalAs(UnmanagedType.U4)] int index, [Out, MarshalAs(UnmanagedType.LPArray)] System.Runtime.InteropServices.ComTypes.TYPEKIND[] pTKind);
            [SecurityCritical]
            void GetTypeInfoOfGuid([In] ref Guid guid, [Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeInfo[] ppTInfo);
            [SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            void RemoteGetLibAttr([Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.tagTLIBATTR[] ppTLibAttr, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pDummy);
            [SecurityCritical]
            void GetTypeComp([Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeComp[] ppTComp);
            [SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            void RemoteGetDocumentation(int index, [In, MarshalAs(UnmanagedType.U4)] int refPtrFlags, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrName, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrDocString, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pdwHelpContext, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrHelpFile);
            [SecurityCritical, Obsolete("Bad signature. Fix and verify signature before use.", true)]
            void RemoteIsName([In, MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pfName, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrLibName);
            [Obsolete("Bad signature. Fix and verify signature before use.", true), SecurityCritical]
            void RemoteFindName([In, MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] UnsafeNativeMethods.ITypeInfo[] ppTInfo, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgMemId, [In, Out, MarshalAs(UnmanagedType.LPArray)] short[] pcFound, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrLibName);
            [Obsolete("Bad signature. Fix and verify signature before use.", true), SecurityCritical]
            void LocalReleaseTLibAttr();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public enum tagSYSKIND
        {
            SYS_MAC = 2,
            SYS_WIN16 = 0
        }

        [StructLayout(LayoutKind.Sequential), EditorBrowsable(EditorBrowsableState.Never)]
        public struct tagTLIBATTR
        {
            public Guid guid;
            public int lcid;
            public UnsafeNativeMethods.tagSYSKIND syskind;
            [MarshalAs(UnmanagedType.U2)]
            public short wMajorVerNum;
            [MarshalAs(UnmanagedType.U2)]
            public short wMinorVerNum;
            [MarshalAs(UnmanagedType.U2)]
            public short wLibFlags;
        }
    }
}

