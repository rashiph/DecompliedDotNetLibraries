namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class PinnedBufferMemoryStream : UnmanagedMemoryStream
    {
        private byte[] _array;
        private GCHandle _pinningHandle;

        [SecurityCritical]
        private PinnedBufferMemoryStream()
        {
        }

        [SecurityCritical]
        internal unsafe PinnedBufferMemoryStream(byte[] array)
        {
            int length = array.Length;
            if (length == 0)
            {
                array = new byte[1];
                length = 0;
            }
            this._array = array;
            this._pinningHandle = new GCHandle(array, GCHandleType.Pinned);
            fixed (byte* numRef = this._array)
            {
                base.Initialize(numRef, (long) length, (long) length, FileAccess.Read, true);
            }
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            if (base._isOpen)
            {
                this._pinningHandle.Free();
                base._isOpen = false;
            }
            base.Dispose(disposing);
        }

        ~PinnedBufferMemoryStream()
        {
            this.Dispose(false);
        }
    }
}

