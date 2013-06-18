namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TraceGuidRegistration
    {
        internal unsafe System.Guid* Guid;
        internal unsafe void* RegHandle;
    }
}

