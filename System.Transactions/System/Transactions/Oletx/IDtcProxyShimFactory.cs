namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Transactions;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("467C8BCB-BDDE-4885-B143-317107468275")]
    internal interface IDtcProxyShimFactory
    {
        void ConnectToProxy([MarshalAs(UnmanagedType.LPWStr)] string nodeName, Guid resourceManagerIdentifier, IntPtr managedIdentifier, [MarshalAs(UnmanagedType.Bool)] out bool nodeNameMatches, [MarshalAs(UnmanagedType.U4)] out uint whereaboutsSize, out CoTaskMemHandle whereaboutsBuffer, [MarshalAs(UnmanagedType.Interface)] out IResourceManagerShim resourceManagerShim);
        void GetNotification(out IntPtr managedIdentifier, [MarshalAs(UnmanagedType.I4)] out ShimNotificationType shimNotificationType, [MarshalAs(UnmanagedType.Bool)] out bool isSinglePhase, [MarshalAs(UnmanagedType.Bool)] out bool abortingHint, [MarshalAs(UnmanagedType.Bool)] out bool releaseRequired, [MarshalAs(UnmanagedType.U4)] out uint prepareInfoSize, out CoTaskMemHandle prepareInfo);
        void ReleaseNotificationLock();
        void BeginTransaction([MarshalAs(UnmanagedType.U4)] uint timeout, OletxTransactionIsolationLevel isolationLevel, IntPtr managedIdentifier, out Guid transactionIdentifier, [MarshalAs(UnmanagedType.Interface)] out ITransactionShim transactionShim);
        void CreateResourceManager(Guid resourceManagerIdentifier, IntPtr managedIdentifier, [MarshalAs(UnmanagedType.Interface)] out IResourceManagerShim resourceManagerShim);
        void Import([MarshalAs(UnmanagedType.U4)] uint cookieSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] cookie, IntPtr managedIdentifier, out Guid transactionIdentifier, out OletxTransactionIsolationLevel isolationLevel, [MarshalAs(UnmanagedType.Interface)] out ITransactionShim transactionShim);
        void ReceiveTransaction([MarshalAs(UnmanagedType.U4)] uint propgationTokenSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] propgationToken, IntPtr managedIdentifier, out Guid transactionIdentifier, out OletxTransactionIsolationLevel isolationLevel, [MarshalAs(UnmanagedType.Interface)] out ITransactionShim transactionShim);
        void CreateTransactionShim([MarshalAs(UnmanagedType.Interface)] IDtcTransaction transactionNative, IntPtr managedIdentifier, out Guid transactionIdentifier, out OletxTransactionIsolationLevel isolationLevel, [MarshalAs(UnmanagedType.Interface)] out ITransactionShim transactionShim);
    }
}

