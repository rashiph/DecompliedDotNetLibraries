namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        internal static int E_FAIL = -2147467259;
        internal static int S_OK = 0;
        internal static int XACT_E_ABORTED = -2147168231;
        internal static int XACT_E_ALREADYINPROGRESS = -2147168232;
        internal static int XACT_E_CONNECTION_DOWN = -2147168228;
        internal static int XACT_E_FIRST = -2147168256;
        internal static int XACT_E_INDOUBT = -2147168234;
        internal static int XACT_E_LAST = -2147168215;
        internal static int XACT_E_NETWORK_TX_DISABLED = -2147168220;
        internal static int XACT_E_NOTRANSACTION = -2147168242;
        internal static int XACT_E_NOTSUPPORTED = -2147168241;
        internal static int XACT_E_PROTOCOL = -2147167995;
        internal static int XACT_E_RECOVERYALREADYDONE = -2147167996;
        internal static int XACT_E_REENLISTTIMEOUT = -2147168226;
        internal static int XACT_E_TMNOTAVAILABLE = -2147168229;
        internal static int XACT_E_TOOMANY_ENLISTMENTS = -2147167999;
        internal static int XACT_S_READONLY = 0x4d002;
        internal static int XACT_S_SINGLEPHASE = 0x4d009;

        [DllImport("System.Transactions.Dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
        internal static extern int GetNotificationFactory(SafeHandle notificationEventHandle, [MarshalAs(UnmanagedType.Interface)] out IDtcProxyShimFactory ppProxyShimFactory);
    }
}

