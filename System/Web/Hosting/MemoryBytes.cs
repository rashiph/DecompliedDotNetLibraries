namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;

    internal class MemoryBytes
    {
        private byte[] _arrayData;
        private System.Web.Hosting.BufferType _bufferType;
        private IntPtr _fileHandle;
        private string _fileName;
        private long _fileSize;
        private IntPtr _intptrData;
        private long _offset;
        private GCHandle _pinnedArrayData;
        private int _size;

        internal MemoryBytes(byte[] data, int size) : this(data, size, false, 0L)
        {
        }

        internal MemoryBytes(IntPtr data, int size, System.Web.Hosting.BufferType bufferType)
        {
            this._size = size;
            this._arrayData = null;
            this._intptrData = data;
            this._fileHandle = IntPtr.Zero;
            this._bufferType = bufferType;
        }

        internal MemoryBytes(string fileName, long offset, long fileSize)
        {
            this._bufferType = System.Web.Hosting.BufferType.TransmitFile;
            this._intptrData = IntPtr.Zero;
            this._fileHandle = IntPtr.Zero;
            this._fileSize = fileSize;
            this._fileName = fileName;
            this._offset = offset;
            this._size = IntPtr.Size;
        }

        internal MemoryBytes(byte[] data, int size, bool useTransmitFile, long fileSize)
        {
            this._size = size;
            this._arrayData = data;
            this._intptrData = IntPtr.Zero;
            this._fileHandle = IntPtr.Zero;
            if (useTransmitFile)
            {
                this._bufferType = System.Web.Hosting.BufferType.TransmitFile;
            }
            this._fileSize = fileSize;
        }

        private void CloseHandle()
        {
            if ((this._fileHandle != IntPtr.Zero) && (this._fileHandle != UnsafeNativeMethods.INVALID_HANDLE_VALUE))
            {
                UnsafeNativeMethods.CloseHandle(this._fileHandle);
                this._fileHandle = IntPtr.Zero;
            }
        }

        private static byte[] IntPtrToBytes(IntPtr p, long offset, long length)
        {
            byte[] buffer = new byte[0x10 + IntPtr.Size];
            for (int i = 0; i < 8; i++)
            {
                buffer[i] = (byte) ((offset >> (8 * i)) & 0xffL);
            }
            for (int j = 0; j < 8; j++)
            {
                buffer[8 + j] = (byte) ((length >> (8 * j)) & 0xffL);
            }
            if (IntPtr.Size == 4)
            {
                int num3 = p.ToInt32();
                for (int m = 0; m < 4; m++)
                {
                    buffer[0x10 + m] = (byte) ((num3 >> (8 * m)) & 0xff);
                }
                return buffer;
            }
            long num5 = p.ToInt64();
            for (int k = 0; k < 8; k++)
            {
                buffer[0x10 + k] = (byte) ((num5 >> (8 * k)) & 0xffL);
            }
            return buffer;
        }

        internal IntPtr LockMemory()
        {
            this.SetHandle();
            if (this._arrayData != null)
            {
                this._pinnedArrayData = GCHandle.Alloc(this._arrayData, GCHandleType.Pinned);
                return Marshal.UnsafeAddrOfPinnedArrayElement(this._arrayData, 0);
            }
            return this._intptrData;
        }

        private void SetHandle()
        {
            if (this._fileName != null)
            {
                this._fileHandle = UnsafeNativeMethods.GetFileHandleForTransmitFile(this._fileName);
            }
            if (this._fileHandle != IntPtr.Zero)
            {
                this._arrayData = IntPtrToBytes(this._fileHandle, this._offset, this._fileSize);
            }
        }

        internal void UnlockMemory()
        {
            this.CloseHandle();
            if (this._arrayData != null)
            {
                this._pinnedArrayData.Free();
            }
        }

        internal System.Web.Hosting.BufferType BufferType
        {
            get
            {
                return this._bufferType;
            }
        }

        internal long FileSize
        {
            get
            {
                return this._fileSize;
            }
        }

        internal bool IsBufferFromUnmanagedPool
        {
            get
            {
                return (this._bufferType == System.Web.Hosting.BufferType.UnmanagedPool);
            }
        }

        internal int Size
        {
            get
            {
                return this._size;
            }
        }

        internal bool UseTransmitFile
        {
            get
            {
                return (this._bufferType == System.Web.Hosting.BufferType.TransmitFile);
            }
        }
    }
}

