namespace System.Web
{
    using System;
    using System.Text;
    using System.Web.Util;

    internal sealed class HttpResponseBufferElement : HttpBaseMemoryResponseBufferElement, IHttpResponseElement
    {
        private byte[] _data;
        private static UbyteBufferAllocator s_Allocator = new UbyteBufferAllocator(0x7c00, 0x40);

        internal HttpResponseBufferElement(byte[] data, int size)
        {
            this._data = data;
            base._size = size;
            base._free = 0;
            base._recycle = false;
        }

        internal override int Append(byte[] data, int offset, int size)
        {
            if ((base._free == 0) || (size == 0))
            {
                return 0;
            }
            int count = (base._free >= size) ? size : base._free;
            Buffer.BlockCopy(data, offset, this._data, base._size - base._free, count);
            base._free -= count;
            return count;
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
            int num = encoder.GetBytes(data, offset, size, this._data, base._size - base._free, flushEncoder);
            base._free -= num;
        }

        internal override HttpResponseBufferElement Clone()
        {
            int count = base._size - base._free;
            byte[] dst = new byte[count];
            Buffer.BlockCopy(this._data, 0, dst, 0, count);
            return new HttpResponseBufferElement(dst, count);
        }

        internal override void Recycle()
        {
        }

        byte[] IHttpResponseElement.GetBytes()
        {
            return this._data;
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
                wr.SendResponseFromMemory(this._data, length);
            }
        }
    }
}

