namespace System.Data.ProviderBase
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal abstract class DbBuffer : SafeHandle
    {
        private readonly int _bufferLength;
        internal const int LMEM_FIXED = 0;
        internal const int LMEM_MOVEABLE = 2;
        internal const int LMEM_ZEROINIT = 0x40;

        protected DbBuffer(int initialSize) : this(initialSize, true)
        {
        }

        private DbBuffer(int initialSize, bool zeroBuffer) : base(IntPtr.Zero, true)
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
                    base.handle = SafeNativeMethods.LocalAlloc(flags, (IntPtr) initialSize);
                }
                if (IntPtr.Zero == base.handle)
                {
                    throw new OutOfMemoryException();
                }
            }
        }

        protected DbBuffer(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
        {
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
                IntPtr ptr = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
                int len = UnsafeNativeMethods.lstrlenW(ptr);
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
                str = Marshal.PtrToStringUni(ADP.IntPtrOffset(base.DangerousGetHandle(), offset), length);
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

        internal byte ReadByte(int offset)
        {
            byte num;
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                num = Marshal.ReadByte(base.DangerousGetHandle(), offset);
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
                Marshal.Copy(ADP.IntPtrOffset(base.DangerousGetHandle(), offset), destination, startIndex, length);
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

        internal char ReadChar(int offset)
        {
            return (char) ((ushort) this.ReadInt16(offset));
        }

        internal char[] ReadChars(int offset, char[] destination, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 2 * length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.Copy(ADP.IntPtrOffset(base.DangerousGetHandle(), offset), destination, startIndex, length);
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

        internal DateTime ReadDate(int offset)
        {
            short[] destination = new short[3];
            this.ReadInt16Array(offset, destination, 0, 3);
            return new DateTime((ushort) destination[0], (ushort) destination[1], (ushort) destination[2]);
        }

        internal DateTime ReadDateTime(int offset)
        {
            short[] destination = new short[6];
            this.ReadInt16Array(offset, destination, 0, 6);
            int num = this.ReadInt32(offset + 12);
            DateTime time = new DateTime((ushort) destination[0], (ushort) destination[1], (ushort) destination[2], (ushort) destination[3], (ushort) destination[4], (ushort) destination[5]);
            return time.AddTicks((long) (num / 100));
        }

        internal double ReadDouble(int offset)
        {
            return BitConverter.Int64BitsToDouble(this.ReadInt64(offset));
        }

        internal Guid ReadGuid(int offset)
        {
            byte[] destination = new byte[0x10];
            this.ReadBytes(offset, destination, 0, 0x10);
            return new Guid(destination);
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

        internal void ReadInt16Array(int offset, short[] destination, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 2 * length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.Copy(ADP.IntPtrOffset(base.DangerousGetHandle(), offset), destination, startIndex, length);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
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

        internal void ReadInt32Array(int offset, int[] destination, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 4 * length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.Copy(ADP.IntPtrOffset(base.DangerousGetHandle(), offset), destination, startIndex, length);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        internal long ReadInt64(int offset)
        {
            long num;
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                num = Marshal.ReadInt64(base.DangerousGetHandle(), offset);
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

        internal decimal ReadNumeric(int offset)
        {
            byte[] destination = new byte[20];
            this.ReadBytes(offset, destination, 1, 0x13);
            int[] bits = new int[4];
            bits[3] = destination[2] << 0x10;
            if (destination[3] == 0)
            {
                bits[3] |= -2147483648;
            }
            bits[0] = BitConverter.ToInt32(destination, 4);
            bits[1] = BitConverter.ToInt32(destination, 8);
            bits[2] = BitConverter.ToInt32(destination, 12);
            if (BitConverter.ToInt32(destination, 0x10) != 0)
            {
                throw ADP.NumericToDecimalOverflow();
            }
            return new decimal(bits);
        }

        internal unsafe float ReadSingle(int offset)
        {
            return *(((float*) &this.ReadInt32(offset)));
        }

        internal TimeSpan ReadTime(int offset)
        {
            short[] destination = new short[3];
            this.ReadInt16Array(offset, destination, 0, 3);
            return new TimeSpan((ushort) destination[0], (ushort) destination[1], (ushort) destination[2]);
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                SafeNativeMethods.LocalFree(handle);
            }
            return true;
        }

        private void StructureToPtr(int offset, object structure)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
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
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer);
            }
        }

        [Conditional("DEBUG")]
        protected void ValidateCheck(int offset, int count)
        {
            this.Validate(offset, count);
        }

        internal void WriteByte(int offset, byte value)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.WriteByte(base.DangerousGetHandle(), offset, value);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
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
                IntPtr destination = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
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

        internal void WriteCharArray(int offset, char[] source, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 2 * length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr destination = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
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

        internal void WriteDate(int offset, DateTime value)
        {
            short[] source = new short[] { (short) value.Year, (short) value.Month, (short) value.Day };
            this.WriteInt16Array(offset, source, 0, 3);
        }

        internal void WriteDateTime(int offset, DateTime value)
        {
            int num = ((int) (value.Ticks % 0x989680L)) * 100;
            short[] source = new short[] { (short) value.Year, (short) value.Month, (short) value.Day, (short) value.Hour, (short) value.Minute, (short) value.Second };
            this.WriteInt16Array(offset, source, 0, 6);
            this.WriteInt32(offset + 12, num);
        }

        internal void WriteDouble(int offset, double value)
        {
            this.WriteInt64(offset, BitConverter.DoubleToInt64Bits(value));
        }

        internal void WriteGuid(int offset, Guid value)
        {
            this.StructureToPtr(offset, value);
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

        internal void WriteInt16Array(int offset, short[] source, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 2 * length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr destination = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
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

        internal void WriteInt32Array(int offset, int[] source, int startIndex, int length)
        {
            offset += this.BaseOffset;
            this.Validate(offset, 4 * length);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr destination = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
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

        internal void WriteInt64(int offset, long value)
        {
            offset += this.BaseOffset;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.WriteInt64(base.DangerousGetHandle(), offset, value);
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

        internal void WriteNumeric(int offset, decimal value, byte precision)
        {
            int[] bits = decimal.GetBits(value);
            byte[] dst = new byte[20];
            dst[1] = precision;
            Buffer.BlockCopy(bits, 14, dst, 2, 2);
            dst[3] = (dst[3] == 0) ? ((byte) 1) : ((byte) 0);
            Buffer.BlockCopy(bits, 0, dst, 4, 12);
            dst[0x10] = 0;
            dst[0x11] = 0;
            dst[0x12] = 0;
            dst[0x13] = 0;
            this.WriteBytes(offset, dst, 1, 0x13);
        }

        internal unsafe void WriteSingle(int offset, float value)
        {
            this.WriteInt32(offset, *((int*) &value));
        }

        internal void WriteTime(int offset, TimeSpan value)
        {
            short[] source = new short[] { (short) value.Hours, (short) value.Minutes, (short) value.Seconds };
            this.WriteInt16Array(offset, source, 0, 3);
        }

        internal void ZeroMemory()
        {
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                SafeNativeMethods.ZeroMemory(base.DangerousGetHandle(), (IntPtr) this.Length);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        private int BaseOffset
        {
            get
            {
                return 0;
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

