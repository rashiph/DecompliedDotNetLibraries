namespace System.IO
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    public class UnmanagedMemoryAccessor : IDisposable
    {
        private FileAccess _access;
        [SecurityCritical]
        private SafeBuffer _buffer;
        private bool _canRead;
        private bool _canWrite;
        private long _capacity;
        private bool _isOpen;
        private long _offset;

        protected UnmanagedMemoryAccessor()
        {
            this._isOpen = false;
        }

        [SecuritySafeCritical]
        public UnmanagedMemoryAccessor(SafeBuffer buffer, long offset, long capacity)
        {
            this.Initialize(buffer, offset, capacity, FileAccess.Read);
        }

        [SecuritySafeCritical]
        public UnmanagedMemoryAccessor(SafeBuffer buffer, long offset, long capacity, FileAccess access)
        {
            this.Initialize(buffer, offset, capacity, access);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._isOpen = false;
        }

        private void EnsureSafeToRead(long position, int sizeOfType)
        {
            if (!this._isOpen)
            {
                throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("ObjectDisposed_ViewAccessorClosed"));
            }
            if (!this.CanRead)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_Reading"));
            }
            if (position < 0L)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (position > (this._capacity - sizeOfType))
            {
                if (position >= this._capacity)
                {
                    throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_PositionLessThanCapacityRequired"));
                }
                throw new ArgumentException(Environment.GetResourceString("Argument_NotEnoughBytesToRead"), "position");
            }
        }

        private void EnsureSafeToWrite(long position, int sizeOfType)
        {
            if (!this._isOpen)
            {
                throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("ObjectDisposed_ViewAccessorClosed"));
            }
            if (!this.CanWrite)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_Writing"));
            }
            if (position < 0L)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (position > (this._capacity - sizeOfType))
            {
                if (position >= this._capacity)
                {
                    throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_PositionLessThanCapacityRequired"));
                }
                throw new ArgumentException(Environment.GetResourceString("Argument_NotEnoughBytesToWrite", new object[] { "Byte" }), "position");
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected unsafe void Initialize(SafeBuffer buffer, long offset, long capacity, FileAccess access)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0L)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (capacity < 0L)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (buffer.ByteLength < (offset + capacity))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_OffsetAndCapacityOutOfBounds"));
            }
            if ((access < FileAccess.Read) || (access > FileAccess.ReadWrite))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (this._isOpen)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CalledTwice"));
            }
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                buffer.AcquirePointer(ref pointer);
                if (((IntPtr) ((((ulong) pointer) + offset) + ((ulong) capacity))) < pointer)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_UnmanagedMemAccessorWrapAround"));
                }
            }
            finally
            {
                if (pointer != null)
                {
                    buffer.ReleasePointer();
                }
            }
            this._offset = offset;
            this._buffer = buffer;
            this._capacity = capacity;
            this._access = access;
            this._isOpen = true;
            this._canRead = (this._access & FileAccess.Read) != 0;
            this._canWrite = (this._access & FileAccess.Write) != 0;
        }

        [SecuritySafeCritical]
        private unsafe byte InternalReadByte(long position)
        {
            byte num;
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                num = *((byte*) ((pointer + this._offset) + position));
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num;
        }

        [SecuritySafeCritical]
        private unsafe void InternalWrite(long position, byte value)
        {
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                (pointer + this._offset)[(int) position] = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecurityCritical]
        public void Read<T>(long position, out T structure) where T: struct
        {
            if (position < 0L)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (!this._isOpen)
            {
                throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("ObjectDisposed_ViewAccessorClosed"));
            }
            if (!this.CanRead)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_Reading"));
            }
            uint num = Marshal.SizeOf<T>();
            if (position > (this._capacity - num))
            {
                if (position >= this._capacity)
                {
                    throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_PositionLessThanCapacityRequired"));
                }
                throw new ArgumentException(Environment.GetResourceString("Argument_NotEnoughBytesToRead", new object[] { typeof(T).FullName }), "position");
            }
            structure = this._buffer.Read<T>((ulong) (this._offset + position));
        }

        [SecurityCritical]
        public int ReadArray<T>(long position, T[] array, int offset, int count) where T: struct
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", "Buffer cannot be null.");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_OffsetAndLengthOutOfBounds"));
            }
            if (!this.CanRead)
            {
                if (!this._isOpen)
                {
                    throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("ObjectDisposed_ViewAccessorClosed"));
                }
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_Reading"));
            }
            if (position < 0L)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            uint num = Marshal.AlignedSizeOf<T>();
            if (position >= this._capacity)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_PositionLessThanCapacityRequired"));
            }
            int num2 = count;
            long num3 = this._capacity - position;
            if (num3 < 0L)
            {
                num2 = 0;
            }
            else
            {
                ulong num4 = (ulong) (num * count);
                if (num3 < num4)
                {
                    num2 = (int) (num3 / ((ulong) num));
                }
            }
            this._buffer.ReadArray<T>((ulong) (this._offset + position), array, offset, num2);
            return num2;
        }

        public bool ReadBoolean(long position)
        {
            int sizeOfType = 1;
            this.EnsureSafeToRead(position, sizeOfType);
            return (this.InternalReadByte(position) != 0);
        }

        public byte ReadByte(long position)
        {
            int sizeOfType = 1;
            this.EnsureSafeToRead(position, sizeOfType);
            return this.InternalReadByte(position);
        }

        [SecuritySafeCritical]
        public unsafe char ReadChar(long position)
        {
            char ch;
            int sizeOfType = 2;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                ch = *((char*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return ch;
        }

        [SecuritySafeCritical]
        public decimal ReadDecimal(long position)
        {
            int sizeOfType = 0x10;
            this.EnsureSafeToRead(position, sizeOfType);
            int[] array = new int[4];
            this.ReadArray<int>(position, array, 0, array.Length);
            return new decimal(array);
        }

        [SecuritySafeCritical]
        public unsafe double ReadDouble(long position)
        {
            double num2;
            int sizeOfType = 8;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((double*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [SecuritySafeCritical]
        public unsafe short ReadInt16(long position)
        {
            short num2;
            int sizeOfType = 2;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((short*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [SecuritySafeCritical]
        public unsafe int ReadInt32(long position)
        {
            int num2;
            int sizeOfType = 4;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((int*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [SecuritySafeCritical]
        public unsafe long ReadInt64(long position)
        {
            long num2;
            int sizeOfType = 8;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((long*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public unsafe sbyte ReadSByte(long position)
        {
            sbyte num2;
            int sizeOfType = 1;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((sbyte*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [SecuritySafeCritical]
        public unsafe float ReadSingle(long position)
        {
            float num2;
            int sizeOfType = 4;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((float*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public unsafe ushort ReadUInt16(long position)
        {
            ushort num2;
            int sizeOfType = 2;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((ushort*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public unsafe uint ReadUInt32(long position)
        {
            uint num2;
            int sizeOfType = 4;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((uint*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public unsafe ulong ReadUInt64(long position)
        {
            ulong num2;
            int sizeOfType = 8;
            this.EnsureSafeToRead(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                num2 = *((ulong*) pointer);
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
            return num2;
        }

        public void Write(long position, bool value)
        {
            int sizeOfType = 1;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte num2 = value ? ((byte) 1) : ((byte) 0);
            this.InternalWrite(position, num2);
        }

        public void Write(long position, byte value)
        {
            int sizeOfType = 1;
            this.EnsureSafeToWrite(position, sizeOfType);
            this.InternalWrite(position, value);
        }

        [SecuritySafeCritical]
        public unsafe void Write(long position, char value)
        {
            int sizeOfType = 2;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((short*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecuritySafeCritical]
        public void Write(long position, decimal value)
        {
            int sizeOfType = 0x10;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte[] buffer = new byte[0x10];
            decimal.GetBytes(value, buffer);
            int[] array = new int[4];
            int num2 = ((buffer[12] | (buffer[13] << 8)) | (buffer[14] << 0x10)) | (buffer[15] << 0x18);
            int num3 = ((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18);
            int num4 = ((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 0x10)) | (buffer[7] << 0x18);
            int num5 = ((buffer[8] | (buffer[9] << 8)) | (buffer[10] << 0x10)) | (buffer[11] << 0x18);
            array[0] = num3;
            array[1] = num4;
            array[2] = num5;
            array[3] = num2;
            this.WriteArray<int>(position, array, 0, array.Length);
        }

        [SecuritySafeCritical]
        public unsafe void Write(long position, double value)
        {
            int sizeOfType = 8;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((double*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecuritySafeCritical]
        public unsafe void Write(long position, short value)
        {
            int sizeOfType = 2;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((short*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecuritySafeCritical]
        public unsafe void Write(long position, int value)
        {
            int sizeOfType = 4;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((int*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecuritySafeCritical]
        public unsafe void Write(long position, long value)
        {
            int sizeOfType = 8;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((long*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public unsafe void Write(long position, sbyte value)
        {
            int sizeOfType = 1;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                pointer[0] = (byte) value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecuritySafeCritical]
        public unsafe void Write(long position, float value)
        {
            int sizeOfType = 4;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((float*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public unsafe void Write(long position, ushort value)
        {
            int sizeOfType = 2;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((short*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public unsafe void Write(long position, uint value)
        {
            int sizeOfType = 4;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((int*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public unsafe void Write(long position, ulong value)
        {
            int sizeOfType = 8;
            this.EnsureSafeToWrite(position, sizeOfType);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._buffer.AcquirePointer(ref pointer);
                pointer += (byte*) (this._offset + position);
                *((long*) pointer) = value;
            }
            finally
            {
                if (pointer != null)
                {
                    this._buffer.ReleasePointer();
                }
            }
        }

        [SecurityCritical]
        public void Write<T>(long position, ref T structure) where T: struct
        {
            if (position < 0L)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (!this._isOpen)
            {
                throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("ObjectDisposed_ViewAccessorClosed"));
            }
            if (!this.CanWrite)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_Writing"));
            }
            uint num = Marshal.SizeOf<T>();
            if (position > (this._capacity - num))
            {
                if (position >= this._capacity)
                {
                    throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_PositionLessThanCapacityRequired"));
                }
                throw new ArgumentException(Environment.GetResourceString("Argument_NotEnoughBytesToWrite", new object[] { typeof(T).FullName }), "position");
            }
            this._buffer.Write<T>((ulong) (this._offset + position), structure);
        }

        [SecurityCritical]
        public void WriteArray<T>(long position, T[] array, int offset, int count) where T: struct
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", "Buffer cannot be null.");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_OffsetAndLengthOutOfBounds"));
            }
            if (position < 0L)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (position >= this.Capacity)
            {
                throw new ArgumentOutOfRangeException("position", Environment.GetResourceString("ArgumentOutOfRange_PositionLessThanCapacityRequired"));
            }
            if (!this._isOpen)
            {
                throw new ObjectDisposedException("UnmanagedMemoryAccessor", Environment.GetResourceString("ObjectDisposed_ViewAccessorClosed"));
            }
            if (!this.CanWrite)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_Writing"));
            }
            this._buffer.WriteArray<T>((ulong) (this._offset + position), array, offset, count);
        }

        public bool CanRead
        {
            get
            {
                return (this._isOpen && this._canRead);
            }
        }

        public bool CanWrite
        {
            get
            {
                return (this._isOpen && this._canWrite);
            }
        }

        public long Capacity
        {
            get
            {
                return this._capacity;
            }
        }

        protected bool IsOpen
        {
            get
            {
                return this._isOpen;
            }
        }
    }
}

