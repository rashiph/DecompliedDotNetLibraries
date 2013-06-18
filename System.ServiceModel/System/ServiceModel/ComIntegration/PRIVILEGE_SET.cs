namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class PRIVILEGE_SET
    {
        internal uint PrivilegeCount = 1;
        internal uint Control;
        internal LUID_AND_ATTRIBUTES Privilege;
    }
}

