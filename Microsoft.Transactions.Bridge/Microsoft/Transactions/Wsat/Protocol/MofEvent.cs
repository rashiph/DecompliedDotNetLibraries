namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size=0x40)]
    internal struct MofEvent
    {
        [FieldOffset(0)]
        internal EventTraceHeader Header;
        [FieldOffset(0x30)]
        internal MofField Mof;
    }
}

