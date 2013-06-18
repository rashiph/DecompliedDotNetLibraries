namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class NETLOGON_INFO_2
    {
        public int netlog2_flags;
        public int netlog2_pdc_connection_status;
        public IntPtr netlog2_trusted_dc_name;
        public int netlog2_tc_connection_status;
    }
}

