using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), DebugInfoInPDB, NativeCppClass, MiscellaneousBits(0x41)]
internal struct gcroot<System::Reflection::MethodBase ^>
{
    public static unsafe void <MarshalCopy>(gcroot<System::Reflection::MethodBase ^>* e ^>Ptr2, gcroot<System::Reflection::MethodBase ^>* e ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) e ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) e ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<System::Reflection::MethodBase ^>* e ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) e ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) e ^>Ptr1) = 0;
    }
}

