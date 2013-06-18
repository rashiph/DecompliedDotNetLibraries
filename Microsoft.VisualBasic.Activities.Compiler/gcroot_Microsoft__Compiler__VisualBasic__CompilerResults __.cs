using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), MiscellaneousBits(0x41), DebugInfoInPDB, NativeCppClass]
internal struct gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^>
{
    public static unsafe void <MarshalCopy>(gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^>* s ^>Ptr2, gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^>* s ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) s ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) s ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<Microsoft::Compiler::VisualBasic::CompilerResults ^>* s ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) s ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) s ^>Ptr1) = 0;
    }
}

