namespace System.Web.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        internal const int ERROR_NO_SUCH_DOMAIN = 0x54b;
        internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;

        [DllImport("Netapi32.dll", EntryPoint="DsGetDcNameW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern int DsGetDcName([In] string computerName, [In] string domainName, [In] IntPtr domainGuid, [In] string siteName, [In] uint flags, out IntPtr domainControllerInfo);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        public static extern int FormatMessageW([In] int dwFlags, [In] int lpSource, [In] int dwMessageId, [In] int dwLanguageId, [Out] StringBuilder lpBuffer, [In] int nSize, [In] int arguments);
        [DllImport("Netapi32.dll")]
        internal static extern int NetApiBufferFree([In] IntPtr buffer);
    }
}

