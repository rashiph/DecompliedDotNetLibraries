using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), NativeCppClass, MiscellaneousBits(0x41), DebugInfoInPDB]
internal struct gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^>
{
    public static unsafe void <MarshalCopy>(gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^>* t ^>Ptr2, gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^>* t ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) t ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) t ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<Microsoft::Compiler::VisualBasic::CompilerContext ^>* t ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) t ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) t ^>Ptr1) = 0;
    }
}

