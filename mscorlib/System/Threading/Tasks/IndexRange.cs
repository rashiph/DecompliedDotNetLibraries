namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IndexRange
    {
        internal long m_nFromInclusive;
        internal long m_nToExclusive;
        internal Shared<long> m_nSharedCurrentIndexOffset;
        internal int m_bRangeFinished;
    }
}

