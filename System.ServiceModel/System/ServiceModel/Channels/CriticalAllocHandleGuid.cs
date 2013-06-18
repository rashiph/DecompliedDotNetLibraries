namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal class CriticalAllocHandleGuid : CriticalAllocHandle
    {
        public static CriticalAllocHandle FromGuid(Guid input)
        {
            int size = Marshal.SizeOf(typeof(Guid));
            CriticalAllocHandle handle = CriticalAllocHandle.FromSize(size);
            Marshal.Copy(input.ToByteArray(), 0, (IntPtr) handle, size);
            return handle;
        }
    }
}

