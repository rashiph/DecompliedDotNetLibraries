namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal class CriticalAllocHandleBlob : CriticalAllocHandle
    {
        public static CriticalAllocHandle FromBlob<T>(T id)
        {
            CriticalAllocHandle handle = CriticalAllocHandle.FromSize(Marshal.SizeOf(typeof(T)));
            Marshal.StructureToPtr(id, (IntPtr) handle, false);
            return handle;
        }
    }
}

