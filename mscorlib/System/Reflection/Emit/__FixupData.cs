namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct __FixupData
    {
        internal Label m_fixupLabel;
        internal int m_fixupPos;
        internal int m_fixupInstSize;
    }
}

