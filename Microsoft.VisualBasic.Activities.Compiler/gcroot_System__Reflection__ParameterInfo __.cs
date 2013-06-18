using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), MiscellaneousBits(0x41), DebugInfoInPDB, NativeCppClass]
internal struct gcroot<System::Reflection::ParameterInfo ^>
{
    public static unsafe void <MarshalCopy>(gcroot<System::Reflection::ParameterInfo ^>* o ^>Ptr2, gcroot<System::Reflection::ParameterInfo ^>* o ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) o ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) o ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<System::Reflection::ParameterInfo ^>* o ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) o ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) o ^>Ptr1) = 0;
    }
}

