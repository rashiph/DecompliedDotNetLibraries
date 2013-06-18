using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), MiscellaneousBits(0x41), NativeCppClass, DebugInfoInPDB]
internal struct gcroot<System::String ^>
{
    public static unsafe void <MarshalCopy>(gcroot<System::String ^>* g ^>Ptr2, gcroot<System::String ^>* g ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) g ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) g ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<System::String ^>* g ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) g ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) g ^>Ptr1) = 0;
    }
}

