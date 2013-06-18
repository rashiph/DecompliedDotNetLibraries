namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;

    internal class ServerVarCharBuffer
    {
        private char[] _charBuffer = ((char[]) s_CharBufferAllocator.GetBuffer());
        private bool _pinned;
        private IntPtr _pinnedAddr;
        private GCHandle _pinnedCharBufferHandle;
        private bool _recyclable = true;
        private const int BUFFER_SIZE = 0x400;
        private const int MAX_FREE_BUFFERS = 0x40;
        private static CharBufferAllocator s_CharBufferAllocator = new CharBufferAllocator(0x400, 0x40);

        internal ServerVarCharBuffer()
        {
        }

        internal void Dispose()
        {
            if (this._pinned)
            {
                this._pinnedCharBufferHandle.Free();
                this._pinned = false;
            }
            if (this._recyclable && (this._charBuffer != null))
            {
                s_CharBufferAllocator.ReuseBuffer(this._charBuffer);
            }
            this._charBuffer = null;
        }

        internal void Resize(int newSize)
        {
            if (this._pinned)
            {
                this._pinnedCharBufferHandle.Free();
                this._pinned = false;
            }
            this._charBuffer = new char[newSize];
            this._recyclable = false;
        }

        internal int Length
        {
            get
            {
                return this._charBuffer.Length;
            }
        }

        internal IntPtr PinnedAddress
        {
            get
            {
                if (!this._pinned)
                {
                    this._pinnedCharBufferHandle = GCHandle.Alloc(this._charBuffer, GCHandleType.Pinned);
                    this._pinnedAddr = Marshal.UnsafeAddrOfPinnedArrayElement(this._charBuffer, 0);
                    this._pinned = true;
                }
                return this._pinnedAddr;
            }
        }
    }
}

