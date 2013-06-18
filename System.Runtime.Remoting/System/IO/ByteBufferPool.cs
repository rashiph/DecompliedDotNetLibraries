namespace System.IO
{
    using System;
    using System.Threading;

    internal class ByteBufferPool : IByteBufferPool
    {
        private byte[][] _bufferPool;
        private int _bufferSize;
        private object _controlCookie = "cookie object";
        private int _current;
        private int _last;
        private int _max;

        public ByteBufferPool(int maxBuffers, int bufferSize)
        {
            this._max = maxBuffers;
            this._bufferPool = new byte[this._max][];
            this._bufferSize = bufferSize;
            this._current = -1;
            this._last = -1;
        }

        public byte[] GetBuffer()
        {
            object obj2 = null;
            byte[] buffer2;
            try
            {
                obj2 = Interlocked.Exchange(ref this._controlCookie, null);
                if (obj2 != null)
                {
                    if (this._current == -1)
                    {
                        this._controlCookie = obj2;
                        return new byte[this._bufferSize];
                    }
                    byte[] buffer = this._bufferPool[this._current];
                    this._bufferPool[this._current] = null;
                    if (this._current == this._last)
                    {
                        this._current = -1;
                    }
                    else
                    {
                        this._current = (this._current + 1) % this._max;
                    }
                    this._controlCookie = obj2;
                    return buffer;
                }
                buffer2 = new byte[this._bufferSize];
            }
            catch (ThreadAbortException)
            {
                if (obj2 != null)
                {
                    this._current = -1;
                    this._last = -1;
                    this._controlCookie = obj2;
                }
                throw;
            }
            return buffer2;
        }

        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            object obj2 = null;
            try
            {
                obj2 = Interlocked.Exchange(ref this._controlCookie, null);
                if (obj2 != null)
                {
                    if (this._current == -1)
                    {
                        this._bufferPool[0] = buffer;
                        this._current = 0;
                        this._last = 0;
                    }
                    else
                    {
                        int num = (this._last + 1) % this._max;
                        if (num != this._current)
                        {
                            this._last = num;
                            this._bufferPool[this._last] = buffer;
                        }
                    }
                    this._controlCookie = obj2;
                }
            }
            catch (ThreadAbortException)
            {
                if (obj2 != null)
                {
                    this._current = -1;
                    this._last = -1;
                    this._controlCookie = obj2;
                }
                throw;
            }
        }
    }
}

