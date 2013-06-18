namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size=0x60)]
    internal struct Mof3Event
    {
        [FieldOffset(0)]
        internal EventTraceHeader Header;
        [FieldOffset(0x30)]
        internal MofField Mof1;
        [FieldOffset(0x40)]
        internal MofField Mof2;
        [FieldOffset(80)]
        internal MofField Mof3;
    }
}

