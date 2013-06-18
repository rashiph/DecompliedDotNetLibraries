using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), DebugInfoInPDB, MiscellaneousBits(0x41), NativeCppClass]
internal struct gcroot<cli::array<System::Type ^ >^>
{
    public static unsafe void <MarshalCopy>(gcroot<cli::array<System::Type ^ >^>* e ^ >^>Ptr2, gcroot<cli::array<System::Type ^ >^>* e ^ >^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) e ^ >^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) e ^ >^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<cli::array<System::Type ^ >^>* e ^ >^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) e ^ >^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) e ^ >^>Ptr1) = 0;
    }
}

