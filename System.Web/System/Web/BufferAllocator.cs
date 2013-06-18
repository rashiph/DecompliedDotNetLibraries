namespace System.Web
{
    using System;
    using System.Collections;

    internal abstract class BufferAllocator
    {
        private Stack _buffers = new Stack();
        private int _maxFree;
        private int _numFree = 0;
        private static int s_ProcsFudgeFactor = SystemInfo.GetNumProcessCPUs();

        static BufferAllocator()
        {
            if (s_ProcsFudgeFactor < 1)
            {
                s_ProcsFudgeFactor = 1;
            }
            if (s_ProcsFudgeFactor > 4)
            {
                s_ProcsFudgeFactor = 4;
            }
        }

        internal BufferAllocator(int maxFree)
        {
            this._maxFree = maxFree * s_ProcsFudgeFactor;
        }

        protected abstract object AllocBuffer();
        internal object GetBuffer()
        {
            object obj2 = null;
            if (this._numFree > 0)
            {
                lock (this)
                {
                    if (this._numFree > 0)
                    {
                        obj2 = this._buffers.Pop();
                        this._numFree--;
                    }
                }
            }
            if (obj2 == null)
            {
                obj2 = this.AllocBuffer();
            }
            return obj2;
        }

        internal void ReuseBuffer(object buffer)
        {
            if (this._numFree < this._maxFree)
            {
                lock (this)
                {
                    if (this._numFree < this._maxFree)
                    {
                        this._buffers.Push(buffer);
                        this._numFree++;
                    }
                }
            }
        }
    }
}

