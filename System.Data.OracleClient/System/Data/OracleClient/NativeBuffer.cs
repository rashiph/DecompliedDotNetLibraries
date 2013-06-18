namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class NativeBuffer : System.Data.ProviderBase.DbBuffer
    {
        public NativeBuffer(int initialSize) : base(initialSize, false)
        {
        }

        public NativeBuffer(int initialSize, bool zeroBuffer) : base(initialSize, zeroBuffer)
        {
        }

        internal IntPtr DangerousGetDataPtr()
        {
            return base.DangerousGetHandle();
        }

        internal IntPtr DangerousGetDataPtr(int offset)
        {
            return System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
        }

        internal IntPtr DangerousGetDataPtrWithBaseOffset(int offset)
        {
            return System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset + base.BaseOffset);
        }

        internal static IntPtr HandleValueToTrace(NativeBuffer buffer)
        {
            return buffer.DangerousGetHandle();
        }

        internal string PtrToStringAnsi(int offset)
        {
            offset += base.BaseOffset;
            base.Validate(offset, 1);
            string str = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
                int len = System.Data.Common.UnsafeNativeMethods.lstrlenA(ptr);
                str = Marshal.PtrToStringAnsi(ptr, len);
                base.Validate(offset, len + 1);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return str;
        }

        internal string PtrToStringAnsi(int offset, int length)
        {
            offset += base.BaseOffset;
            base.Validate(offset, length);
            string str = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                str = Marshal.PtrToStringAnsi(System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset), length);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return str;
        }

        internal object PtrToStructure(int offset, Type oftype)
        {
            offset += base.BaseOffset;
            object obj2 = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                obj2 = Marshal.PtrToStructure(System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset), oftype);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return obj2;
        }

        internal static void SafeDispose(ref NativeBuffer_LongColumnData handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }
    }
}

