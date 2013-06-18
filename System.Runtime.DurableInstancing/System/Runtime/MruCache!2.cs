namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class MruCache<TKey, TValue> where TKey: class where TValue: class
    {
        private int highWatermark;
        private Dictionary<TKey, CacheEntry<TKey, TValue>> items;
        private int lowWatermark;
        private CacheEntry<TKey, TValue> mruEntry;
        private LinkedList<TKey> mruList;

        public MruCache(int watermark) : this((watermark * 4) / 5, watermark)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MruCache(int lowWatermark, int highWatermark) : this(lowWatermark, highWatermark, null)
        {
        }

        public MruCache(int lowWatermark, int highWatermark, IEqualityComparer<TKey> comparer)
        {
            this.lowWatermark = lowWatermark;
            this.highWatermark = highWatermark;
            this.mruList = new LinkedList<TKey>();
            if (comparer == null)
            {
                this.items = new Dictionary<TKey, CacheEntry<TKey, TValue>>();
            }
            else
            {
                this.items = new Dictionary<TKey, CacheEntry<TKey, TValue>>(comparer);
            }
        }

        public void Add(TKey key, TValue value)
        {
            bool flag = false;
            try
            {
                CacheEntry<TKey, TValue> entry;
                if (this.items.Count == this.highWatermark)
                {
                    int num = this.highWatermark - this.lowWatermark;
                    for (int i = 0; i < num; i++)
                    {
                        TKey local = this.mruList.Last.Value;
                        this.mruList.RemoveLast();
                        TValue item = this.items[local].value;
                        this.items.Remove(local);
                        this.OnSingleItemRemoved(item);
                    }
                }
                entry.node = this.mruList.AddFirst(key);
                entry.value = value;
                this.items.Add(key, entry);
                this.mruEntry = entry;
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    this.Clear();
                }
            }
        }

        public void Clear()
        {
            this.mruList.Clear();
            this.items.Clear();
            this.mruEntry.value = default(TValue);
            this.mruEntry.node = null;
        }

        protected virtual void OnSingleItemRemoved(TValue item)
        {
        }

        public bool Remove(TKey key)
        {
            CacheEntry<TKey, TValue> entry;
            if (!this.items.TryGetValue(key, out entry))
            {
                return false;
            }
            this.items.Remove(key);
            this.OnSingleItemRemoved(entry.value);
            this.mruList.Remove(entry.node);
            if (object.ReferenceEquals(this.mruEntry.node, entry.node))
            {
                this.mruEntry.value = default(TValue);
                this.mruEntry.node = null;
            }
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            CacheEntry<TKey, TValue> entry;
            if (((this.mruEntry.node != null) && (key != null)) && key.Equals(this.mruEntry.node.Value))
            {
                value = this.mruEntry.value;
                return true;
            }
            bool flag = this.items.TryGetValue(key, out entry);
            value = entry.value;
            if ((flag && (this.mruList.Count > 1)) && !object.ReferenceEquals(this.mruList.First, entry.node))
            {
                this.mruList.Remove(entry.node);
                this.mruList.AddFirst(entry.node);
                this.mruEntry = entry;
            }
            return flag;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CacheEntry
        {
            internal TValue value;
            internal LinkedListNode<TKey> node;
        }
    }
}

