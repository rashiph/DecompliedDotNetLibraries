namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size=80)]
    internal struct Guid2Event
    {
        [FieldOffset(0x30)]
        internal Guid Guid1;
        [FieldOffset(0x40)]
        internal Guid Guid2;
        [FieldOffset(0)]
        internal EventTraceHeader Header;
    }
}

