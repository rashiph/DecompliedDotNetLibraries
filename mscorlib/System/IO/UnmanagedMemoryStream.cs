namespace System.IO
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public class UnmanagedMemoryStream : Stream
    {
        private FileAccess _access;
        [SecurityCritical]
        private SafeBuffer _buffer;
        private long _capacity;
        internal bool _isOpen;
        private long _length;
        private unsafe byte* _mem;
        private long _offset;
        private long _position;
        private const long UnmanagedMemStreamMaxLength = 0x7fffffffffffffffL;

        [SecuritySafeCritical]
        protected unsafe UnmanagedMemoryStream()
        {
            this._mem = null;
            this._isOpen = false;
        }

        [SecurityCritical, CLSCompliant(false)]
        public unsafe UnmanagedMemoryStream(byte* pointer, long length)
        {
            this.Initialize(pointer, length, length, FileAccess.Read, false);
        }

        [SecuritySafeCritical]
        public UnmanagedMemoryStream(SafeBuffer buffer, long offset, long length)
        {
            this.Initialize(buffer, offset, length, FileAccess.Read, false);
        }

        [SecuritySafeCritical]
        public UnmanagedMemoryStream(SafeBuffer buffer, long offset, long length, FileAccess access)
        {
            this.Initialize(buffer, offset, length, access, false);
        }

        [SecurityCritical, CLSCompliant(false)]
        public unsafe UnmanagedMemoryStream(byte* pointer, long length, long capacity, FileAccess access)
        {
            this.Initialize(pointer, length, capacity, access, false);
        }

        [SecurityCritical]
        internal UnmanagedMemoryStream(SafeBuffer buffer, long offset, long length, FileAccess access, bool skipSecurityCheck)
        {
            this.Initialize(buffer, offset, length, access, skipSecurityCheck);
        }

        [SecurityCritical]
        internal unsafe UnmanagedMemoryStream(byte* pointer, long length, long capacity, FileAccess access, bool skipSecurityCheck)
        {
            this.Initialize(pointer, length, capacity, access, skipSecurityCheck);
        }

        [SecuritySafeCritical]
        protected override unsafe void Dispose(bool disposing)
        {
            this._isOpen = false;
            this._mem = null;
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
        }

        [SecuritySafeCritical]
        protected void Initialize(SafeBuffer buffer, long offset, long length, FileAccess access)
        {
            this.Initialize(buffer, offset, length, access, false);
        }

        [SecurityCritical, CLSCompliant(false)]
        protected unsafe void Initialize(byte* pointer, long length, long capacity, FileAccess access)
        {
            this.Initialize(pointer, length, capacity, access, false);
        }

        [SecurityCritical]
        internal unsafe void Initialize(SafeBuffer buffer, long offset, long length, FileAccess access, bool skipSecurityCheck)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0L)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (length < 0L)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (buffer.ByteLength < (offset + length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSafeBufferOffLen"));
            }
            if ((access < FileAccess.Read) || (access > FileAccess.ReadWrite))
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (this._isOpen)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CalledTwice"));
            }
            if (!skipSecurityCheck)
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                buffer.AcquirePointer(ref pointer);
                if (((pointer + offset) + length) < pointer)
                {
                    throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamWrapAround"));
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
            this._length = length;
            this._capacity = length;
            this._access = access;
            this._isOpen = true;
        }

        [SecurityCritical]
        internal unsafe void Initialize(byte* pointer, long length, long capacity, FileAccess access, bool skipSecurityCheck)
        {
            if (pointer == null)
            {
                throw new ArgumentNullException("pointer");
            }
            if ((length < 0L) || (capacity < 0L))
            {
                throw new ArgumentOutOfRangeException((length < 0L) ? "length" : "capacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (length > capacity)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_LengthGreaterThanCapacity"));
            }
            if (((IntPtr) (((ulong) pointer) + capacity)) < pointer)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamWrapAround"));
            }
            if ((access < FileAccess.Read) || (access > FileAccess.ReadWrite))
            {
                throw new ArgumentOutOfRangeException("access", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            if (this._isOpen)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CalledTwice"));
            }
            if (!skipSecurityCheck)
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            this._mem = pointer;
            this._offset = 0L;
            this._length = length;
            this._capacity = capacity;
            this._access = access;
            this._isOpen = true;
        }

        [SecuritySafeCritical]
        public override unsafe int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
            if (!this.CanRead)
            {
                __Error.ReadNotSupported();
            }
            long num = Interlocked.Read(ref this._position);
            long num3 = Interlocked.Read(ref this._length) - num;
            if (num3 > count)
            {
                num3 = count;
            }
            if (num3 <= 0L)
            {
                return 0;
            }
            int len = (int) num3;
            if (len < 0)
            {
                len = 0;
            }
            if (this._buffer != null)
            {
                byte* pointer = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this._buffer.AcquirePointer(ref pointer);
                    Buffer.memcpy((byte*) ((pointer + num) + this._offset), 0, buffer, offset, len);
                }
                finally
                {
                    if (pointer != null)
                    {
                        this._buffer.ReleasePointer();
                    }
                }
            }
            else
            {
                Buffer.memcpy(this._mem + ((byte*) num), 0, buffer, offset, len);
            }
            Interlocked.Exchange(ref this._position, num + num3);
            return len;
        }

        [SecuritySafeCritical]
        public override unsafe int ReadByte()
        {
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
            if (!this.CanRead)
            {
                __Error.ReadNotSupported();
            }
            long num = Interlocked.Read(ref this._position);
            long num2 = Interlocked.Read(ref this._length);
            if (num >= num2)
            {
                return -1;
            }
            Interlocked.Exchange(ref this._position, num + 1L);
            if (this._buffer != null)
            {
                byte* pointer = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this._buffer.AcquirePointer(ref pointer);
                    return *(((int*) ((pointer + num) + this._offset)));
                }
                finally
                {
                    if (pointer != null)
                    {
                        this._buffer.ReleasePointer();
                    }
                }
            }
            return *(((int*) (this._mem + num)));
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
            if (offset > 0x7fffffffffffffffL)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamLength"));
            }
            switch (loc)
            {
                case SeekOrigin.Begin:
                    if (offset < 0L)
                    {
                        throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
                    }
                    Interlocked.Exchange(ref this._position, offset);
                    break;

                case SeekOrigin.Current:
                {
                    long num = Interlocked.Read(ref this._position);
                    if ((offset + num) < 0L)
                    {
                        throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
                    }
                    Interlocked.Exchange(ref this._position, offset + num);
                    break;
                }
                case SeekOrigin.End:
                {
                    long num2 = Interlocked.Read(ref this._length);
                    if ((num2 + offset) < 0L)
                    {
                        throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
                    }
                    Interlocked.Exchange(ref this._position, num2 + offset);
                    break;
                }
                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSeekOrigin"));
            }
            return Interlocked.Read(ref this._position);
        }

        [SecuritySafeCritical]
        public override unsafe void SetLength(long value)
        {
            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this._buffer != null)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_UmsSafeBuffer"));
            }
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
            if (value > this._capacity)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_FixedCapacity"));
            }
            long num = Interlocked.Read(ref this._position);
            long num2 = Interlocked.Read(ref this._length);
            if (value > num2)
            {
                Buffer.ZeroMemory(this._mem + ((byte*) num2), value - num2);
            }
            Interlocked.Exchange(ref this._length, value);
            if (num > value)
            {
                Interlocked.Exchange(ref this._position, value);
            }
        }

        [SecuritySafeCritical]
        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
            long num = Interlocked.Read(ref this._position);
            long num2 = Interlocked.Read(ref this._length);
            long num3 = num + count;
            if (num3 < 0L)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_StreamTooLong"));
            }
            if (num3 > this._capacity)
            {
                throw new NotSupportedException(Environment.GetResourceString("IO.IO_FixedCapacity"));
            }
            if (this._buffer == null)
            {
                if (num > num2)
                {
                    Buffer.ZeroMemory(this._mem + ((byte*) num2), num - num2);
                }
                if (num3 > num2)
                {
                    Interlocked.Exchange(ref this._length, num3);
                }
            }
            if (this._buffer != null)
            {
                long num4 = this._capacity - num;
                if (num4 < count)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_BufferTooSmall"));
                }
                byte* pointer = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this._buffer.AcquirePointer(ref pointer);
                    Buffer.memcpy(buffer, offset, (byte*) ((pointer + num) + this._offset), 0, count);
                }
                finally
                {
                    if (pointer != null)
                    {
                        this._buffer.ReleasePointer();
                    }
                }
            }
            else
            {
                Buffer.memcpy(buffer, offset, this._mem + ((byte*) num), 0, count);
            }
            Interlocked.Exchange(ref this._position, num3);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteByte(byte value)
        {
            if (!this._isOpen)
            {
                __Error.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
            long num = Interlocked.Read(ref this._position);
            long num2 = Interlocked.Read(ref this._length);
            long num3 = num + 1L;
            if (num >= num2)
            {
                if (num3 < 0L)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_StreamTooLong"));
                }
                if (num3 > this._capacity)
                {
                    throw new NotSupportedException(Environment.GetResourceString("IO.IO_FixedCapacity"));
                }
                if (this._buffer == null)
                {
                    if (num > num2)
                    {
                        Buffer.ZeroMemory(this._mem + ((byte*) num2), num - num2);
                    }
                    Interlocked.Exchange(ref this._length, num3);
                }
            }
            if (this._buffer != null)
            {
                byte* pointer = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this._buffer.AcquirePointer(ref pointer);
                    (pointer + num)[(int) this._offset] = value;
                }
                finally
                {
                    if (pointer != null)
                    {
                        this._buffer.ReleasePointer();
                    }
                }
            }
            else
            {
                this._mem[(int) num] = value;
            }
            Interlocked.Exchange(ref this._position, num3);
        }

        public override bool CanRead
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this._isOpen && ((this._access & FileAccess.Read) != 0));
            }
        }

        public override bool CanSeek
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this._isOpen;
            }
        }

        public override bool CanWrite
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this._isOpen && ((this._access & FileAccess.Write) != 0));
            }
        }

        public long Capacity
        {
            get
            {
                if (!this._isOpen)
                {
                    __Error.StreamIsClosed();
                }
                return this._capacity;
            }
        }

        public override long Length
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (!this._isOpen)
                {
                    __Error.StreamIsClosed();
                }
                return Interlocked.Read(ref this._length);
            }
        }

        internal byte* Pointer
        {
            [SecurityCritical]
            get
            {
                if (this._buffer != null)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_UmsSafeBuffer"));
                }
                return this._mem;
            }
        }

        public override long Position
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (!this.CanSeek)
                {
                    __Error.StreamIsClosed();
                }
                return Interlocked.Read(ref this._position);
            }
            [SecuritySafeCritical]
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                }
                if (!this.CanSeek)
                {
                    __Error.StreamIsClosed();
                }
                if ((value > 0x7fffffffL) || ((this._mem + value) < this._mem))
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_StreamLength"));
                }
                Interlocked.Exchange(ref this._position, value);
            }
        }

        [CLSCompliant(false)]
        public byte* PositionPointer
        {
            [SecurityCritical]
            get
            {
                if (this._buffer != null)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_UmsSafeBuffer"));
                }
                long num = Interlocked.Read(ref this._position);
                if (num > this._capacity)
                {
                    throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_UMSPosition"));
                }
                byte* numPtr = this._mem + ((byte*) num);
                if (!this._isOpen)
                {
                    __Error.StreamIsClosed();
                }
                return numPtr;
            }
            [SecurityCritical]
            set
            {
                if (this._buffer != null)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_UmsSafeBuffer"));
                }
                if (!this._isOpen)
                {
                    __Error.StreamIsClosed();
                }
                IntPtr ptr = new IntPtr((long) ((value - this._mem) / 1));
                if (ptr.ToInt64() > 0x7fffffffffffffffL)
                {
                    throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_UnmanagedMemStreamLength"));
                }
                if (value < this._mem)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_SeekBeforeBegin"));
                }
                Interlocked.Exchange(ref this._position, (long) ((value - this._mem) / 1));
            }
        }
    }
}

