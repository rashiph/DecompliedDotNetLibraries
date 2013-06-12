namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RangePositionInfo
    {
        public BitSet curpos;
        public decimal[] rangeCounters;
    }
}

