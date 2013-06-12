namespace System.IO.MemoryMappedFiles
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class MemoryMappedView : IDisposable
    {
        private MemoryMappedFileAccess m_access;
        private long m_pointerOffset;
        private long m_size;
        private SafeMemoryMappedViewHandle m_viewHandle;

        [SecurityCritical]
        private MemoryMappedView(SafeMemoryMappedViewHandle viewHandle, long pointerOffset, long size, MemoryMappedFileAccess access)
        {
            this.m_viewHandle = viewHandle;
            this.m_pointerOffset = pointerOffset;
            this.m_size = size;
            this.m_access = access;
        }

        [SecurityCritical]
        internal static MemoryMappedView CreateView(SafeMemoryMappedFileHandle memMappedFileHandle, MemoryMappedFileAccess access, long offset, long size)
        {
            ulong num3;
            ulong num = (ulong) (offset % ((long) MemoryMappedFile.GetSystemPageAllocationGranularity()));
            ulong num2 = ((ulong) offset) - num;
            if (size != 0L)
            {
                num3 = ((ulong) size) + num;
            }
            else
            {
                num3 = 0L;
            }
            if ((IntPtr.Size == 4) && (num3 > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("size", System.SR.GetString("ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed"));
            }
            Microsoft.Win32.UnsafeNativeMethods.MEMORYSTATUSEX lpBuffer = new Microsoft.Win32.UnsafeNativeMethods.MEMORYSTATUSEX();
            Microsoft.Win32.UnsafeNativeMethods.GlobalMemoryStatusEx(lpBuffer);
            ulong ullTotalVirtual = lpBuffer.ullTotalVirtual;
            if (num3 >= ullTotalVirtual)
            {
                throw new IOException(System.SR.GetString("IO_NotEnoughMemory"));
            }
            uint dwFileOffsetLow = (uint) (num2 & 0xffffffffL);
            uint dwFileOffsetHigh = (uint) (num2 >> 0x20);
            SafeMemoryMappedViewHandle address = Microsoft.Win32.UnsafeNativeMethods.MapViewOfFile(memMappedFileHandle, MemoryMappedFile.GetFileMapAccess(access), dwFileOffsetHigh, dwFileOffsetLow, new UIntPtr(num3));
            if (address.IsInvalid)
            {
                System.IO.__Error.WinIOError(Marshal.GetLastWin32Error(), string.Empty);
            }
            Microsoft.Win32.UnsafeNativeMethods.MEMORY_BASIC_INFORMATION buffer = new Microsoft.Win32.UnsafeNativeMethods.MEMORY_BASIC_INFORMATION();
            Microsoft.Win32.UnsafeNativeMethods.VirtualQuery(address, ref buffer, (IntPtr) Marshal.SizeOf(buffer));
            ulong regionSize = (ulong) buffer.RegionSize;
            if ((buffer.State & 0x2000) != 0)
            {
                Microsoft.Win32.UnsafeNativeMethods.VirtualAlloc(address, (UIntPtr) regionSize, 0x1000, MemoryMappedFile.GetPageAccess(access));
                int errorCode = Marshal.GetLastWin32Error();
                if (address.IsInvalid)
                {
                    System.IO.__Error.WinIOError(errorCode, string.Empty);
                }
            }
            if (size == 0L)
            {
                size = (long) (regionSize - num);
            }
            address.Initialize(((ulong) size) + num);
            return new MemoryMappedView(address, (long) num, size, access);
        }

        [SecurityCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecurityCritical]
        protected virtual void Dispose(bool disposing)
        {
            if ((this.m_viewHandle != null) && !this.m_viewHandle.IsClosed)
            {
                this.m_viewHandle.Dispose();
            }
        }

        [SecurityCritical]
        public unsafe void Flush(IntPtr capacity)
        {
            if (this.m_viewHandle != null)
            {
                byte* pointer = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this.m_viewHandle.AcquirePointer(ref pointer);
                    if (!Microsoft.Win32.UnsafeNativeMethods.FlushViewOfFile(pointer, capacity))
                    {
                        System.IO.__Error.WinIOError(Marshal.GetLastWin32Error(), string.Empty);
                    }
                }
                finally
                {
                    if (pointer != null)
                    {
                        this.m_viewHandle.ReleasePointer();
                    }
                }
            }
        }

        internal MemoryMappedFileAccess Access
        {
            get
            {
                return this.m_access;
            }
        }

        internal bool IsClosed
        {
            get
            {
                if (this.m_viewHandle != null)
                {
                    return this.m_viewHandle.IsClosed;
                }
                return true;
            }
        }

        internal long PointerOffset
        {
            get
            {
                return this.m_pointerOffset;
            }
        }

        internal long Size
        {
            get
            {
                return this.m_size;
            }
        }

        internal SafeMemoryMappedViewHandle ViewHandle
        {
            [SecurityCritical]
            get
            {
                return this.m_viewHandle;
            }
        }
    }
}

