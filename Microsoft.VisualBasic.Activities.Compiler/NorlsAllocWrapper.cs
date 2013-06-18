using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), MiscellaneousBits(0x40), DebugInfoInPDB, NativeCppClass]
internal struct NorlsAllocWrapper
{
    public static unsafe void <MarshalCopy>(NorlsAllocWrapper* wrapperPtr1, NorlsAllocWrapper* wrapperPtr2)
    {
        *((int*) wrapperPtr1) = *((int*) wrapperPtr2);
    }
}

