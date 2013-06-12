namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    internal sealed class KeyedQueue<K, V>
    {
        private readonly Dictionary<K, Queue<V>> _data;

        internal KeyedQueue()
        {
            this._data = new Dictionary<K, Queue<V>>();
        }

        internal void Clear()
        {
            this._data.Clear();
        }

        internal V Dequeue(K key)
        {
            Queue<V> queue;
            if (!this._data.TryGetValue(key, out queue))
            {
                throw Error.QueueEmpty();
            }
            V local = queue.Dequeue();
            if (queue.Count == 0)
            {
                this._data.Remove(key);
            }
            return local;
        }

        internal void Enqueue(K key, V value)
        {
            Queue<V> queue;
            if (!this._data.TryGetValue(key, out queue))
            {
                this._data.Add(key, queue = new Queue<V>());
            }
            queue.Enqueue(value);
        }

        internal int GetCount(K key)
        {
            Queue<V> queue;
            if (!this._data.TryGetValue(key, out queue))
            {
                return 0;
            }
            return queue.Count;
        }

        internal V Peek(K key)
        {
            Queue<V> queue;
            if (!this._data.TryGetValue(key, out queue))
            {
                throw Error.QueueEmpty();
            }
            return queue.Peek();
        }

        internal bool TryDequeue(K key, out V value)
        {
            Queue<V> queue;
            if (this._data.TryGetValue(key, out queue) && (queue.Count > 0))
            {
                value = queue.Dequeue();
                if (queue.Count == 0)
                {
                    this._data.Remove(key);
                }
                return true;
            }
            value = default(V);
            return false;
        }
    }
}

