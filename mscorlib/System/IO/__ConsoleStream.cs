namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class __ConsoleStream : Stream
    {
        private bool _canRead;
        private bool _canWrite;
        [SecurityCritical]
        private SafeFileHandle _handle;
        internal const int DefaultBufferSize = 0x80;
        private const int ERROR_BROKEN_PIPE = 0x6d;
        private const int ERROR_NO_DATA = 0xe8;

        [SecurityCritical]
        internal __ConsoleStream(SafeFileHandle handle, FileAccess access)
        {
            this._handle = handle;
            this._canRead = access == FileAccess.Read;
            this._canWrite = access == FileAccess.Write;
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            if (this._handle != null)
            {
                this._handle = null;
            }
            this._canRead = false;
            this._canWrite = false;
            base.Dispose(disposing);
        }

        [SecuritySafeCritical]
        public override void Flush()
        {
            if (this._handle == null)
            {
                __Error.FileNotOpen();
            }
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
        }

        [SecuritySafeCritical]
        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((offset < 0) ? "offset" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (!this._canRead)
            {
                __Error.ReadNotSupported();
            }
            int errorCode = 0;
            int num2 = ReadFileNative(this._handle, buffer, offset, count, 0, out errorCode);
            if (num2 == -1)
            {
                __Error.WinIOError(errorCode, string.Empty);
            }
            return num2;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("kernel32.dll", SetLastError=true)]
        private static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);
        [SecurityCritical]
        private static unsafe int ReadFileNative(SafeFileHandle hFile, byte[] bytes, int offset, int count, int mustBeZero, out int errorCode)
        {
            int num;
            int num2;
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_IORaceCondition"));
            }
            if (bytes.Length == 0)
            {
                errorCode = 0;
                return 0;
            }
            WaitForAvailableConsoleInput(hFile);
            fixed (byte* numRef = bytes)
            {
                num = ReadFile(hFile, numRef + offset, count, out num2, Win32Native.NULL);
            }
            if (num == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0x6d)
                {
                    return 0;
                }
                return -1;
            }
            errorCode = 0;
            return num2;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            __Error.SeekNotSupported();
            return 0L;
        }

        public override void SetLength(long value)
        {
            __Error.SeekNotSupported();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void WaitForAvailableConsoleInput(SafeFileHandle file);
        [SecuritySafeCritical]
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((offset < 0) ? "offset" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (!this._canWrite)
            {
                __Error.WriteNotSupported();
            }
            int errorCode = 0;
            if (WriteFileNative(this._handle, buffer, offset, count, 0, out errorCode) == -1)
            {
                __Error.WinIOError(errorCode, string.Empty);
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);
        [SecurityCritical]
        private static unsafe int WriteFileNative(SafeFileHandle hFile, byte[] bytes, int offset, int count, int mustBeZero, out int errorCode)
        {
            int num2;
            if (bytes.Length == 0)
            {
                errorCode = 0;
                return 0;
            }
            int numBytesWritten = 0;
            fixed (byte* numRef = bytes)
            {
                num2 = WriteFile(hFile, numRef + offset, count, out numBytesWritten, Win32Native.NULL);
            }
            if (num2 == 0)
            {
                errorCode = Marshal.GetLastWin32Error();
                if ((errorCode != 0xe8) && (errorCode != 0x6d))
                {
                    return -1;
                }
                return 0;
            }
            errorCode = 0;
            return numBytesWritten;
        }

        public override bool CanRead
        {
            get
            {
                return this._canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._canWrite;
            }
        }

        public override long Length
        {
            get
            {
                __Error.SeekNotSupported();
                return 0L;
            }
        }

        public override long Position
        {
            get
            {
                __Error.SeekNotSupported();
                return 0L;
            }
            set
            {
                __Error.SeekNotSupported();
            }
        }
    }
}

