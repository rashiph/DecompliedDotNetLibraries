namespace System.Data.Common
{
    using System;
    using System.Data.OleDb;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool AddAccessAllowedAce(IntPtr pAcl, int dwAceRevision, uint AccessMask, IntPtr pSid);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool AddAccessDeniedAce(IntPtr pAcl, int dwAceRevision, int AccessMask, IntPtr pSid);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool AllocateAndInitializeSid(IntPtr pIdentifierAuthority, byte nSubAuthorityCount, int dwSubAuthority0, int dwSubAuthority1, int dwSubAuthority2, int dwSubAuthority3, int dwSubAuthority4, int dwSubAuthority5, int dwSubAuthority6, int dwSubAuthority7, ref IntPtr pSid);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        internal static extern IntPtr CreateFileMappingA(IntPtr hFile, IntPtr pAttr, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, [MarshalAs(UnmanagedType.LPStr)] string lpName);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern IntPtr FreeSid(IntPtr pSid);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern int GetLengthSid(IntPtr pSid);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool InitializeSecurityDescriptor(IntPtr pSecurityDescriptor, int dwRevision);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
        internal static extern IntPtr OpenFileMappingA(int dwDesiredAccess, bool bInheritHandle, [MarshalAs(UnmanagedType.LPStr)] string lpName);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool SetSecurityDescriptorDacl(IntPtr pSecurityDescriptor, bool bDaclPresent, IntPtr pDacl, bool bDaclDefaulted);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0c733a1e-2a1c-11ce-ade5-00aa0044773d")]
        internal interface ISourcesRowset
        {
            [PreserveSig]
            OleDbHResult GetSourcesRowset([In] IntPtr pUnkOuter, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [In] int cPropertySets, [In] IntPtr rgProperties, [MarshalAs(UnmanagedType.Interface)] out object ppRowset);
        }

        [ComImport, Guid("0C733A5E-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ITransactionJoin
        {
            [PreserveSig, Obsolete("not used", true)]
            int GetOptionsObject();
            void JoinTransaction([In, MarshalAs(UnmanagedType.Interface)] object punkTransactionCoord, [In] int isoLevel, [In] int isoFlags, [In] IntPtr pOtherOptions);
        }
    }
}

