namespace System.Security
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class SecureString : IDisposable
    {
        private const int BlockSize = 8;
        [SecurityCritical]
        private SafeBSTRHandle m_buffer;
        private bool m_encrypted;
        private int m_length;
        private bool m_readOnly;
        private const int MaxLength = 0x10000;
        private const uint ProtectionScope = 0;
        private static bool supportedOnCurrentPlatform = EncryptionSupported();

        [SecuritySafeCritical]
        public SecureString()
        {
            this.CheckSupportedOnCurrentPlatform();
            this.AllocateBuffer(8);
            this.m_length = 0;
        }

        [SecurityCritical]
        internal SecureString(SecureString str)
        {
            this.AllocateBuffer(str.BufferLength);
            SafeBSTRHandle.Copy(str.m_buffer, this.m_buffer);
            this.m_length = str.m_length;
            this.m_encrypted = str.m_encrypted;
        }

        [CLSCompliant(false), SecurityCritical]
        public unsafe SecureString(char* value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (length > 0x10000)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Length"));
            }
            this.InitializeSecureString(value, length);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private void AllocateBuffer(int size)
        {
            uint alignedSize = GetAlignedSize(size);
            this.m_buffer = SafeBSTRHandle.Allocate(null, alignedSize);
            if (this.m_buffer.IsInvalid)
            {
                throw new OutOfMemoryException();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical, HandleProcessCorruptedStateExceptions]
        public void AppendChar(char c)
        {
            this.EnsureNotDisposed();
            this.EnsureNotReadOnly();
            this.EnsureCapacity(this.m_length + 1);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.UnProtectMemory();
                this.m_buffer.Write<char>((ulong) (this.m_length * 2), c);
                this.m_length++;
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
            }
        }

        private void CheckSupportedOnCurrentPlatform()
        {
            if (!supportedOnCurrentPlatform)
            {
                throw new NotSupportedException(Environment.GetResourceString("Arg_PlatformSecureString"));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical]
        public void Clear()
        {
            this.EnsureNotDisposed();
            this.EnsureNotReadOnly();
            this.m_length = 0;
            this.m_buffer.ClearBuffer();
            this.m_encrypted = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical]
        public SecureString Copy()
        {
            this.EnsureNotDisposed();
            return new SecureString(this);
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical]
        public void Dispose()
        {
            if ((this.m_buffer != null) && !this.m_buffer.IsInvalid)
            {
                this.m_buffer.Close();
                this.m_buffer = null;
            }
        }

        [SecurityCritical]
        private static bool EncryptionSupported()
        {
            bool flag = true;
            try
            {
                Win32Native.SystemFunction041(SafeBSTRHandle.Allocate(null, 0x10), 0x10, 0);
            }
            catch (EntryPointNotFoundException)
            {
                flag = false;
            }
            return flag;
        }

        [SecurityCritical]
        private void EnsureCapacity(int capacity)
        {
            if (capacity > 0x10000)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_Capacity"));
            }
            if (capacity > this.m_buffer.Length)
            {
                SafeBSTRHandle target = SafeBSTRHandle.Allocate(null, GetAlignedSize(capacity));
                if (target.IsInvalid)
                {
                    throw new OutOfMemoryException();
                }
                SafeBSTRHandle.Copy(this.m_buffer, target);
                this.m_buffer.Close();
                this.m_buffer = target;
            }
        }

        [SecurityCritical]
        private void EnsureNotDisposed()
        {
            if (this.m_buffer == null)
            {
                throw new ObjectDisposedException(null);
            }
        }

        private void EnsureNotReadOnly()
        {
            if (this.m_readOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static uint GetAlignedSize(int size)
        {
            uint num = (uint) ((size / 8) * 8);
            if (((size % 8) == 0) && (size != 0))
            {
                return num;
            }
            return (num + 8);
        }

        [SecurityCritical]
        private unsafe int GetAnsiByteCount()
        {
            int num3;
            uint flags = 0x400;
            uint num2 = 0x3f;
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.m_buffer.AcquirePointer(ref pointer);
                num3 = Win32Native.WideCharToMultiByte(0, flags, (char*) pointer, this.m_length, null, 0, IntPtr.Zero, new IntPtr((void*) &num2));
            }
            finally
            {
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
            return num3;
        }

        [SecurityCritical]
        private unsafe void GetAnsiBytes(byte* ansiStrPtr, int byteCount)
        {
            uint flags = 0x400;
            uint num2 = 0x3f;
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.m_buffer.AcquirePointer(ref pointer);
                Win32Native.WideCharToMultiByte(0, flags, (char*) pointer, this.m_length, ansiStrPtr, byteCount - 1, IntPtr.Zero, new IntPtr((void*) &num2));
                *((ansiStrPtr + byteCount) - 1) = 0;
            }
            finally
            {
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
        }

        [SecurityCritical, HandleProcessCorruptedStateExceptions]
        private unsafe void InitializeSecureString(char* value, int length)
        {
            this.CheckSupportedOnCurrentPlatform();
            this.AllocateBuffer(length);
            this.m_length = length;
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.m_buffer.AcquirePointer(ref pointer);
                Buffer.memcpyimpl((byte*) value, pointer, length * 2);
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
            this.ProtectMemory();
        }

        [MethodImpl(MethodImplOptions.Synchronized), HandleProcessCorruptedStateExceptions, SecuritySafeCritical]
        public unsafe void InsertAt(int index, char c)
        {
            if ((index < 0) || (index > this.m_length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_IndexString"));
            }
            this.EnsureNotDisposed();
            this.EnsureNotReadOnly();
            this.EnsureCapacity(this.m_length + 1);
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.UnProtectMemory();
                this.m_buffer.AcquirePointer(ref pointer);
                char* chPtr = (char*) pointer;
                for (int i = this.m_length; i > index; i--)
                {
                    chPtr[i] = chPtr[i - 1];
                }
                chPtr[index] = c;
                this.m_length++;
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical]
        public bool IsReadOnly()
        {
            this.EnsureNotDisposed();
            return this.m_readOnly;
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical]
        public void MakeReadOnly()
        {
            this.EnsureNotDisposed();
            this.m_readOnly = true;
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecurityCritical]
        private void ProtectMemory()
        {
            if ((this.m_length != 0) && !this.m_encrypted)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    int status = Win32Native.SystemFunction040(this.m_buffer, (uint) (this.m_buffer.Length * 2), 0);
                    if (status < 0)
                    {
                        throw new CryptographicException(Win32Native.LsaNtStatusToWinError(status));
                    }
                    this.m_encrypted = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized), HandleProcessCorruptedStateExceptions, SecuritySafeCritical]
        public unsafe void RemoveAt(int index)
        {
            this.EnsureNotDisposed();
            this.EnsureNotReadOnly();
            if ((index < 0) || (index >= this.m_length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_IndexString"));
            }
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.UnProtectMemory();
                this.m_buffer.AcquirePointer(ref pointer);
                char* chPtr = (char*) pointer;
                for (int i = index; i < (this.m_length - 1); i++)
                {
                    chPtr[i] = chPtr[i + 1];
                }
                chPtr[--this.m_length] = '\0';
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical, HandleProcessCorruptedStateExceptions]
        public void SetAt(int index, char c)
        {
            if ((index < 0) || (index >= this.m_length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_IndexString"));
            }
            this.EnsureNotDisposed();
            this.EnsureNotReadOnly();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.UnProtectMemory();
                this.m_buffer.Write<char>((ulong) (index * 2), c);
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized), HandleProcessCorruptedStateExceptions, SecurityCritical]
        internal unsafe IntPtr ToAnsiStr(bool allocateFromHeap)
        {
            this.EnsureNotDisposed();
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            int cb = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.UnProtectMemory();
                cb = this.GetAnsiByteCount() + 1;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (allocateFromHeap)
                    {
                        zero = Marshal.AllocHGlobal(cb);
                    }
                    else
                    {
                        zero = Marshal.AllocCoTaskMem(cb);
                    }
                }
                if (zero == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                this.GetAnsiBytes((byte*) zero.ToPointer(), cb);
                ptr2 = zero;
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
                if ((ptr2 == IntPtr.Zero) && (zero != IntPtr.Zero))
                {
                    Win32Native.ZeroMemory(zero, (uint) cb);
                    if (allocateFromHeap)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                    else
                    {
                        Marshal.FreeCoTaskMem(zero);
                    }
                }
            }
            return ptr2;
        }

        [MethodImpl(MethodImplOptions.Synchronized), HandleProcessCorruptedStateExceptions, SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal unsafe IntPtr ToBSTR()
        {
            this.EnsureNotDisposed();
            int length = this.m_length;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    zero = Win32Native.SysAllocStringLen(null, length);
                }
                if (zero == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                this.UnProtectMemory();
                this.m_buffer.AcquirePointer(ref pointer);
                Buffer.memcpyimpl(pointer, (byte*) zero.ToPointer(), length * 2);
                ptr2 = zero;
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
                if ((ptr2 == IntPtr.Zero) && (zero != IntPtr.Zero))
                {
                    Win32Native.ZeroMemory(zero, (uint) (length * 2));
                    Win32Native.SysFreeString(zero);
                }
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
            return ptr2;
        }

        [MethodImpl(MethodImplOptions.Synchronized), HandleProcessCorruptedStateExceptions, SecurityCritical]
        internal unsafe IntPtr ToUniStr(bool allocateFromHeap)
        {
            this.EnsureNotDisposed();
            int length = this.m_length;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (allocateFromHeap)
                    {
                        zero = Marshal.AllocHGlobal((int) ((length + 1) * 2));
                    }
                    else
                    {
                        zero = Marshal.AllocCoTaskMem((length + 1) * 2);
                    }
                }
                if (zero == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                this.UnProtectMemory();
                this.m_buffer.AcquirePointer(ref pointer);
                Buffer.memcpyimpl(pointer, (byte*) zero.ToPointer(), length * 2);
                *((short*) (zero.ToPointer() + (length * 2))) = 0;
                ptr2 = zero;
            }
            catch (Exception)
            {
                this.ProtectMemory();
                throw;
            }
            finally
            {
                this.ProtectMemory();
                if ((ptr2 == IntPtr.Zero) && (zero != IntPtr.Zero))
                {
                    Win32Native.ZeroMemory(zero, (uint) (length * 2));
                    if (allocateFromHeap)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                    else
                    {
                        Marshal.FreeCoTaskMem(zero);
                    }
                }
                if (pointer != null)
                {
                    this.m_buffer.ReleasePointer();
                }
            }
            return ptr2;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void UnProtectMemory()
        {
            if (this.m_length != 0)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (this.m_encrypted)
                    {
                        int status = Win32Native.SystemFunction041(this.m_buffer, (uint) (this.m_buffer.Length * 2), 0);
                        if (status < 0)
                        {
                            throw new CryptographicException(Win32Native.LsaNtStatusToWinError(status));
                        }
                        this.m_encrypted = false;
                    }
                }
            }
        }

        private int BufferLength
        {
            [SecurityCritical]
            get
            {
                return this.m_buffer.Length;
            }
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.Synchronized), SecuritySafeCritical]
            get
            {
                this.EnsureNotDisposed();
                return this.m_length;
            }
        }
    }
}

