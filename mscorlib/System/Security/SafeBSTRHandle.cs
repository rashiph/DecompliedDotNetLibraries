namespace System.Security
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    [SecurityCritical, SuppressUnmanagedCodeSecurity]
    internal sealed class SafeBSTRHandle : SafeBuffer
    {
        internal SafeBSTRHandle() : base(true)
        {
        }

        internal static SafeBSTRHandle Allocate(string src, uint len)
        {
            SafeBSTRHandle handle = SysAllocStringLen(src, len);
            handle.Initialize((ulong) (len * 2));
            return handle;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe void ClearBuffer()
        {
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.AcquirePointer(ref pointer);
                Win32Native.ZeroMemory((IntPtr) pointer, (uint) (Win32Native.SysStringLen((IntPtr) pointer) * 2));
            }
            finally
            {
                if (pointer != null)
                {
                    base.ReleasePointer();
                }
            }
        }

        internal static unsafe void Copy(SafeBSTRHandle source, SafeBSTRHandle target)
        {
            byte* pointer = null;
            byte* numPtr2 = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                source.AcquirePointer(ref pointer);
                target.AcquirePointer(ref numPtr2);
                Buffer.memcpyimpl(pointer, numPtr2, Win32Native.SysStringLen((IntPtr) pointer) * 2);
            }
            finally
            {
                if (pointer != null)
                {
                    source.ReleasePointer();
                }
                if (numPtr2 != null)
                {
                    target.ReleasePointer();
                }
            }
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            Win32Native.ZeroMemory(base.handle, (uint) (Win32Native.SysStringLen(base.handle) * 2));
            Win32Native.SysFreeString(base.handle);
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oleaut32.dll", CharSet=CharSet.Unicode)]
        private static extern SafeBSTRHandle SysAllocStringLen(string src, uint len);

        internal int Length
        {
            get
            {
                return Win32Native.SysStringLen(this);
            }
        }
    }
}

