using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), DebugInfoInPDB, NativeCppClass, MiscellaneousBits(0x40)]
internal struct RefCountedPtr<IConstIterator<IUnknown *> >
{
    [StructLayout(LayoutKind.Sequential, Size=8), NativeCppClass, MiscellaneousBits(0x41), DebugInfoInPDB]
    internal struct Data
    {
    }
}

