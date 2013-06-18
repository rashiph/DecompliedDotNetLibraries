namespace System.Data.ProviderBase
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class DbBuffer : SafeHandle
    {
        private int _baseOffset;
        private readonly int _bufferLength;
        internal const int LMEM_FIXED = 0;
        internal const int LMEM_MOVEABLE = 2;
        internal const int LMEM_ZEROINIT = 0x40;

        protected DbBuffer(int initialSize) : this(initialSize, true)
        {
        }

        protected DbBuffer(int initialSize, bool zeroBuffer) : base(IntPtr.Zero, true)
        {
            if (0 < initialSize)
            {
                int flags = zeroBuffer ? 0x40 : 0;
                this._bufferLength = initialSize;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    base.handle = System.Data.Common.SafeNativeMethods.LocalAlloc(flags, (IntPtr) initialSize);
                }
                if (IntPtr.Zero == base.handle)
                {
                    throw new OutOfMemoryException();
                }
            }
        }

        internal string PtrToStringUni(int offset)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 2);
            string str = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
                int len = System.Data.Common.UnsafeNativeMethods.lstrlenW(ptr);
                this.Validate(offset, 2 * (len + 1));
                str = Marshal.PtrToStringUni(ptr, len);
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

        internal string PtrToStringUni(int offset, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 2 * length);
            string str = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                str = Marshal.PtrToStringUni(System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset), length);
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

        internal byte[] ReadBytes(int offset, int length)
        {
            byte[] destination = new byte[length];
            return this.ReadBytes(offset, destination, 0, length);
        }

        internal byte[] ReadBytes(int offset, byte[] destination, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.Copy(System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset), destination, startIndex, length);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return destination;
        }

        internal short ReadInt16(int offset)
        {
            short num;
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                num = Marshal.ReadInt16(base.DangerousGetHandle(), offset);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return num;
        }

        internal int ReadInt32(int offset)
        {
            int num;
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                num = Marshal.ReadInt32(base.DangerousGetHandle(), offset);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return num;
        }

        internal IntPtr ReadIntPtr(int offset)
        {
            IntPtr ptr;
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                ptr = Marshal.ReadIntPtr(base.DangerousGetHandle(), offset);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return ptr;
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                System.Data.Common.SafeNativeMethods.LocalFree(handle);
            }
            return true;
        }

        internal void StructureToPtr(int offset, object structure)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
                Marshal.StructureToPtr(structure, ptr, false);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        protected void Validate(int offset, int count)
        {
            if (((offset < 0) || (count < 0)) || (this.Length < (offset + count)))
            {
                throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidBuffer);
            }
        }

        [Conditional("DEBUG")]
        protected void ValidateCheck(int offset, int count)
        {
            this.Validate(offset, count);
        }

        internal void WriteBytes(int offset, byte[] source, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr destination = System.Data.Common.ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
                Marshal.Copy(source, startIndex, destination, length);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        internal void WriteInt16(int offset, short value)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.WriteInt16(base.DangerousGetHandle(), offset, value);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        internal void WriteInt32(int offset, int value)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.WriteInt32(base.DangerousGetHandle(), offset, value);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        internal void WriteIntPtr(int offset, IntPtr value)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.WriteIntPtr(base.DangerousGetHandle(), offset, value);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        protected int BaseOffset
        {
            get
            {
                return this._baseOffset;
            }
            set
            {
                this._baseOffset = value;
            }
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }

        internal int Length
        {
            get
            {
                return this._bufferLength;
            }
        }
    }
}

