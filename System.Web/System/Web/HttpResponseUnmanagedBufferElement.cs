namespace System.Web
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class HttpResponseUnmanagedBufferElement : HttpBaseMemoryResponseBufferElement, IHttpResponseElement
    {
        private IntPtr _data;
        private static IntPtr s_Pool;

        static HttpResponseUnmanagedBufferElement()
        {
            if (HttpRuntime.UseIntegratedPipeline)
            {
                s_Pool = UnsafeIISMethods.MgdGetBufferPool(BufferingParams.INTEGRATED_MODE_BUFFER_SIZE);
            }
            else
            {
                s_Pool = UnsafeNativeMethods.BufferPoolGetPool(0x7c00, 0x40);
            }
        }

        internal HttpResponseUnmanagedBufferElement()
        {
            if (HttpRuntime.UseIntegratedPipeline)
            {
                this._data = UnsafeIISMethods.MgdGetBuffer(s_Pool);
                base._size = BufferingParams.INTEGRATED_MODE_BUFFER_SIZE;
            }
            else
            {
                this._data = UnsafeNativeMethods.BufferPoolGetBuffer(s_Pool);
                base._size = 0x7c00;
            }
            if (this._data == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            base._free = base._size;
            base._recycle = true;
        }

        internal void AdjustSize(int size)
        {
            base._free -= size;
        }

        internal override int Append(byte[] data, int offset, int size)
        {
            if ((base._free == 0) || (size == 0))
            {
                return 0;
            }
            int num = (base._free >= size) ? size : base._free;
            Misc.CopyMemory(data, offset, this._data, base._size - base._free, num);
            base._free -= num;
            return num;
        }

        internal override int Append(IntPtr data, int offset, int size)
        {
            if ((base._free == 0) || (size == 0))
            {
                return 0;
            }
            int num = (base._free >= size) ? size : base._free;
            Misc.CopyMemory(data, offset, this._data, base._size - base._free, num);
            base._free -= num;
            return num;
        }

        internal override void AppendEncodedChars(char[] data, int offset, int size, System.Text.Encoder encoder, bool flushEncoder)
        {
            int num = UnsafeAppendEncodedChars(data, offset, size, this._data, base._size - base._free, base._free, encoder, flushEncoder);
            base._free -= num;
        }

        internal override HttpResponseBufferElement Clone()
        {
            int size = base._size - base._free;
            byte[] dest = new byte[size];
            Misc.CopyMemory(this._data, 0, dest, 0, size);
            return new HttpResponseBufferElement(dest, size);
        }

        ~HttpResponseUnmanagedBufferElement()
        {
            IntPtr pBuffer = Interlocked.Exchange(ref this._data, IntPtr.Zero);
            if (pBuffer != IntPtr.Zero)
            {
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    UnsafeIISMethods.MgdReturnBuffer(pBuffer);
                }
                else
                {
                    UnsafeNativeMethods.BufferPoolReleaseBuffer(pBuffer);
                }
            }
        }

        private void ForceRecycle()
        {
            IntPtr pBuffer = Interlocked.Exchange(ref this._data, IntPtr.Zero);
            if (pBuffer != IntPtr.Zero)
            {
                base._free = 0;
                base._recycle = false;
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    UnsafeIISMethods.MgdReturnBuffer(pBuffer);
                }
                else
                {
                    UnsafeNativeMethods.BufferPoolReleaseBuffer(pBuffer);
                }
                GC.SuppressFinalize(this);
            }
        }

        internal override void Recycle()
        {
            if (base._recycle)
            {
                this.ForceRecycle();
            }
        }

        byte[] IHttpResponseElement.GetBytes()
        {
            int size = base._size - base._free;
            if (size > 0)
            {
                byte[] dest = new byte[size];
                Misc.CopyMemory(this._data, 0, dest, 0, size);
                return dest;
            }
            return null;
        }

        long IHttpResponseElement.GetSize()
        {
            return (long) (base._size - base._free);
        }

        void IHttpResponseElement.Send(HttpWorkerRequest wr)
        {
            int length = base._size - base._free;
            if (length > 0)
            {
                wr.SendResponseFromMemory(this._data, length, true);
            }
        }

        private static unsafe int UnsafeAppendEncodedChars(char[] src, int srcOffset, int srcSize, IntPtr dest, int destOffset, int destSize, System.Text.Encoder encoder, bool flushEncoder)
        {
            int num = 0;
            byte* bytes = (byte*) (((void*) dest) + destOffset);
            fixed (char* chRef = src)
            {
                num = encoder.GetBytes(chRef + srcOffset, srcSize, bytes, destSize, flushEncoder);
            }
            return num;
        }

        internal IntPtr FreeLocation
        {
            get
            {
                int num = base._size - base._free;
                byte* numPtr = (byte*) (this._data.ToPointer() + num);
                return new IntPtr((void*) numPtr);
            }
        }
    }
}

