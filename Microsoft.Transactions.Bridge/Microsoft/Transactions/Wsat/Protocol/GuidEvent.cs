namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size=0x40)]
    internal struct GuidEvent
    {
        [FieldOffset(0x30)]
        internal System.Guid Guid;
        [FieldOffset(0)]
        internal EventTraceHeader Header;
    }
}

