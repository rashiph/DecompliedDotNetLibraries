namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class EtwNativeMethods
    {
        [SuppressUnmanagedCodeSecurity, DllImport("advapi32", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint GetTraceEnableFlags(ulong traceHandle);
        [SuppressUnmanagedCodeSecurity, DllImport("advapi32", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern byte GetTraceEnableLevel(ulong traceHandle);
        [SuppressUnmanagedCodeSecurity, DllImport("advapi32", EntryPoint="RegisterTraceGuidsW", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint RegisterTraceGuids([In] EtwTraceCallback cbFunc, [In] void* context, [In] ref Guid controlGuid, [In] uint guidCount, ref TraceGuidRegistration guidReg, [In] string mofImagePath, [In] string mofResourceName, out ulong regHandle);
        [SuppressUnmanagedCodeSecurity, DllImport("advapi32", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint TraceEvent(ulong traceHandle, char* header);
        [SuppressUnmanagedCodeSecurity, DllImport("advapi32", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern int UnregisterTraceGuids(ulong regHandle);
    }
}

