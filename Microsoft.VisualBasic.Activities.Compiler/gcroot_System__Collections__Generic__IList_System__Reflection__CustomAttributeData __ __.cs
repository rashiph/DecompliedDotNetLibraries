using Microsoft.VisualC;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size=4), MiscellaneousBits(0x41), DebugInfoInPDB, NativeCppClass]
internal struct gcroot<System::Collections::Generic::IList<System::Reflection::CustomAttributeData ^> ^>
{
    public static unsafe void <MarshalCopy>(gcroot<System::Collections::Generic::IList<System::Reflection::CustomAttributeData ^> ^>* a ^> ^>Ptr2, gcroot<System::Collections::Generic::IList<System::Reflection::CustomAttributeData ^> ^>* a ^> ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) a ^> ^>Ptr1));
        GCHandle handle = (GCHandle) ptr;
        *((int*) a ^> ^>Ptr2) = ((IntPtr) GCHandle.Alloc(handle.Target)).ToPointer();
    }

    public static unsafe void <MarshalDestroy>(gcroot<System::Collections::Generic::IList<System::Reflection::CustomAttributeData ^> ^>* a ^> ^>Ptr1)
    {
        IntPtr ptr = new IntPtr(*((void**) a ^> ^>Ptr1));
        ((GCHandle) ptr).Free();
        *((int*) a ^> ^>Ptr1) = 0;
    }
}

