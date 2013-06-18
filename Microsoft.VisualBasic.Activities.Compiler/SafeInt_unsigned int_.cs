using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), DebugInfoInPDB, MiscellaneousBits(0x40), NativeCppClass]
internal struct SafeInt<unsigned int>
{
    public static unsafe void <MarshalDestroy>(SafeInt<unsigned int>*)
    {
    }
}

