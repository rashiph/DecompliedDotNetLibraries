namespace System.Web
{
    using System;
    using System.Text;

    internal abstract class HttpBaseMemoryResponseBufferElement
    {
        protected int _free;
        protected bool _recycle;
        protected int _size;

        protected HttpBaseMemoryResponseBufferElement()
        {
        }

        internal abstract int Append(byte[] data, int offset, int size);
        internal abstract int Append(IntPtr data, int offset, int size);
        internal abstract void AppendEncodedChars(char[] data, int offset, int size, System.Text.Encoder encoder, bool flushEncoder);
        internal abstract HttpResponseBufferElement Clone();
        internal void DisableRecycling()
        {
            this._recycle = false;
        }

        internal abstract void Recycle();

        internal int FreeBytes
        {
            get
            {
                return this._free;
            }
        }
    }
}

