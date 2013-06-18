namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size=0x10)]
    internal struct MofField
    {
        [FieldOffset(0)]
        internal IntPtr Data;
        [FieldOffset(8)]
        internal uint Length;
        [FieldOffset(12)]
        internal uint Type;
    }
}

