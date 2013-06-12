namespace System.Web
{
    using System;
    using System.Web.Util;

    internal sealed class HttpResourceResponseElement : IHttpResponseElement
    {
        private IntPtr _data;
        private int _offset;
        private int _size;

        internal HttpResourceResponseElement(IntPtr data, int offset, int size)
        {
            this._data = data;
            this._offset = offset;
            this._size = size;
        }

        byte[] IHttpResponseElement.GetBytes()
        {
            if (this._size > 0)
            {
                byte[] dest = new byte[this._size];
                Misc.CopyMemory(this._data, this._offset, dest, 0, this._size);
                return dest;
            }
            return null;
        }

        long IHttpResponseElement.GetSize()
        {
            return (long) this._size;
        }

        void IHttpResponseElement.Send(HttpWorkerRequest wr)
        {
            if (this._size > 0)
            {
                bool isBufferFromUnmanagedPool = false;
                wr.SendResponseFromMemory(new IntPtr(this._data.ToInt64() + this._offset), this._size, isBufferFromUnmanagedPool);
            }
        }
    }
}

