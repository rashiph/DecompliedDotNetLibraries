using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), DebugInfoInPDB, NativeCppClass, MiscellaneousBits(0x40)]
internal struct SafeInt<int>
{
    public static unsafe void <MarshalDestroy>(SafeInt<int>*)
    {
    }
}

