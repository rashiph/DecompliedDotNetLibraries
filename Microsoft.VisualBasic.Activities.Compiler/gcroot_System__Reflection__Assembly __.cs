using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), NativeCppClass, MiscellaneousBits(0x41), DebugInfoInPDB]
internal struct gcroot<System::Reflection::Assembly ^>
{
    public static unsafe void <MarshalCopy>(gcroot<System::Reflection::Assembly ^>* y ^>Ptr2, gcroot<System::Reflection::Assembly ^>* y ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) y ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) y ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<System::Reflection::Assembly ^>* y ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) y ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) y ^>Ptr1) = 0;
    }
}

