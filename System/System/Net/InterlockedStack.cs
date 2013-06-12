namespace System.Net
{
    using System;
    using System.Collections;

    internal sealed class InterlockedStack
    {
        private int _count;
        private readonly Stack _stack = new Stack();

        internal InterlockedStack()
        {
        }

        internal object Pop()
        {
            lock (this._stack.SyncRoot)
            {
                object obj2 = null;
                if (0 < this._stack.Count)
                {
                    obj2 = this._stack.Pop();
                    this._count = this._stack.Count;
                }
                return obj2;
            }
        }

        internal void Push(object pooledStream)
        {
            if (pooledStream == null)
            {
                throw new ArgumentNullException("pooledStream");
            }
            lock (this._stack.SyncRoot)
            {
                this._stack.Push(pooledStream);
                this._count = this._stack.Count;
            }
        }
    }
}

