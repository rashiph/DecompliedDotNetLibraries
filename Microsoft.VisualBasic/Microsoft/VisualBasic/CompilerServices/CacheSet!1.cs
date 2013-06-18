namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Collections.Generic;

    internal sealed class CacheSet<T>
    {
        private readonly Dictionary<T, LinkedListNode<T>> _dict;
        private readonly LinkedList<T> _list;
        private readonly int _maxSize;

        internal CacheSet(int maxSize)
        {
            this._dict = new Dictionary<T, LinkedListNode<T>>();
            this._list = new LinkedList<T>();
            this._maxSize = maxSize;
        }

        internal T GetExistingOrAdd(T key)
        {
            lock (set)
            {
                LinkedListNode<T> node = null;
                if (this._dict.TryGetValue(key, out node))
                {
                    if (node.Previous != null)
                    {
                        this._list.Remove(node);
                        this._list.AddFirst(node);
                    }
                    return node.Value;
                }
                if (this._dict.Count == this._maxSize)
                {
                    this._dict.Remove(this._list.Last.Value);
                    this._list.RemoveLast();
                }
                node = new LinkedListNode<T>(key);
                this._dict.Add(key, node);
                this._list.AddFirst(node);
                return key;
            }
        }
    }
}

