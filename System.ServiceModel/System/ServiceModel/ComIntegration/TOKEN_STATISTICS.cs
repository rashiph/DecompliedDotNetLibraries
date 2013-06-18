namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TOKEN_STATISTICS
    {
        internal LUID TokenId;
        internal LUID AuthenticationId;
        internal long ExpirationTime;
        internal uint TokenType;
        internal SecurityImpersonationLevel ImpersonationLevel;
        internal uint DynamicCharged;
        internal uint DynamicAvailable;
        internal uint GroupCount;
        internal uint PrivilegeCount;
        internal LUID ModifiedId;
    }
}

