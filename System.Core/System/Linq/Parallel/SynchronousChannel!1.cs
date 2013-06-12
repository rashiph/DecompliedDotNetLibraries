namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal sealed class SynchronousChannel<T>
    {
        private Queue<T> m_queue;

        internal SynchronousChannel()
        {
        }

        internal void CopyTo(T[] array, int arrayIndex)
        {
            this.m_queue.CopyTo(array, arrayIndex);
        }

        internal T Dequeue()
        {
            return this.m_queue.Dequeue();
        }

        internal void Enqueue(T item)
        {
            this.m_queue.Enqueue(item);
        }

        internal void Init()
        {
            this.m_queue = new Queue<T>();
        }

        internal void SetDone()
        {
        }

        internal int Count
        {
            get
            {
                return this.m_queue.Count;
            }
        }
    }
}

