namespace std
{
    using Microsoft.VisualC;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=4), NativeCppClass, MiscellaneousBits(0x40), DebugInfoInPDB]
    internal struct locale
    {
        [StructLayout(LayoutKind.Sequential, Size=8), NativeCppClass, CLSCompliant(false), MiscellaneousBits(0x40), DebugInfoInPDB]
        public struct facet
        {
        }
    }
}

