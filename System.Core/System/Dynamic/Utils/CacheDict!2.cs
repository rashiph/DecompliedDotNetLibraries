namespace System.Dynamic.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class CacheDict<TKey, TValue>
    {
        private readonly Dictionary<TKey, KeyInfo<TKey, TValue>> _dict;
        private readonly LinkedList<TKey> _list;
        private readonly int _maxSize;

        internal CacheDict(int maxSize)
        {
            this._dict = new Dictionary<TKey, KeyInfo<TKey, TValue>>();
            this._list = new LinkedList<TKey>();
            this._maxSize = maxSize;
        }

        internal void Add(TKey key, TValue value)
        {
            KeyInfo<TKey, TValue> info;
            if (this._dict.TryGetValue(key, out info))
            {
                this._list.Remove(info.List);
            }
            else if (this._list.Count == this._maxSize)
            {
                LinkedListNode<TKey> last = this._list.Last;
                this._list.RemoveLast();
                this._dict.Remove(last.Value);
            }
            LinkedListNode<TKey> node2 = new LinkedListNode<TKey>(key);
            this._list.AddFirst(node2);
            this._dict[key] = new KeyInfo<TKey, TValue>(value, node2);
        }

        internal bool TryGetValue(TKey key, out TValue value)
        {
            KeyInfo<TKey, TValue> info;
            if (this._dict.TryGetValue(key, out info))
            {
                LinkedListNode<TKey> list = info.List;
                if (list.Previous != null)
                {
                    this._list.Remove(list);
                    this._list.AddFirst(list);
                }
                value = info.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        internal TValue this[TKey key]
        {
            get
            {
                TValue local;
                if (!this.TryGetValue(key, out local))
                {
                    throw new KeyNotFoundException();
                }
                return local;
            }
            set
            {
                this.Add(key, value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyInfo
        {
            internal readonly TValue Value;
            internal readonly LinkedListNode<TKey> List;
            internal KeyInfo(TValue value, LinkedListNode<TKey> list)
            {
                this.Value = value;
                this.List = list;
            }
        }
    }
}

