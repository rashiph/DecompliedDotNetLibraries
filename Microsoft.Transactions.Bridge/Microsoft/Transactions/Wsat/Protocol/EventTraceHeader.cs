namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size=0x30)]
    internal struct EventTraceHeader
    {
        [FieldOffset(0)]
        internal ushort BufferSize;
        [FieldOffset(40)]
        internal uint ClientContext;
        [FieldOffset(0x2c)]
        internal uint Flags;
        [FieldOffset(0x18)]
        internal System.Guid Guid;
        [FieldOffset(8)]
        internal ulong HistoricalContext;
        [FieldOffset(5)]
        internal byte Level;
        [FieldOffset(0x10)]
        internal long TimeStamp;
        [FieldOffset(4)]
        internal byte Type;
        [FieldOffset(6)]
        internal short Version;
    }
}

