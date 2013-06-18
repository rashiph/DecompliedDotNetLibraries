using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), MiscellaneousBits(0x41), NativeCppClass, DebugInfoInPDB]
internal struct gcroot<System::Object ^>
{
    public static unsafe void <MarshalCopy>(gcroot<System::Object ^>* t ^>Ptr2, gcroot<System::Object ^>* t ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) t ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) t ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<System::Object ^>* t ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) t ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) t ^>Ptr1) = 0;
    }
}

