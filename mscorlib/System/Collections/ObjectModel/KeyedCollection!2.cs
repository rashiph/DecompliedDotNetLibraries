namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false), DebuggerTypeProxy(typeof(Mscorlib_KeyedCollectionDebugView<,>)), DebuggerDisplay("Count = {Count}")]
    public abstract class KeyedCollection<TKey, TItem> : Collection<TItem>
    {
        private IEqualityComparer<TKey> comparer;
        private const int defaultThreshold = 0;
        private Dictionary<TKey, TItem> dict;
        private int keyCount;
        private int threshold;

        protected KeyedCollection() : this(null, 0)
        {
        }

        protected KeyedCollection(IEqualityComparer<TKey> comparer) : this(comparer, 0)
        {
        }

        protected KeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }
            if (dictionaryCreationThreshold == -1)
            {
                dictionaryCreationThreshold = 0x7fffffff;
            }
            if (dictionaryCreationThreshold < -1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.dictionaryCreationThreshold, ExceptionResource.ArgumentOutOfRange_InvalidThreshold);
            }
            this.comparer = comparer;
            this.threshold = dictionaryCreationThreshold;
        }

        private void AddKey(TKey key, TItem item)
        {
            if (this.dict != null)
            {
                this.dict.Add(key, item);
            }
            else if (this.keyCount == this.threshold)
            {
                this.CreateDictionary();
                this.dict.Add(key, item);
            }
            else
            {
                if (this.Contains(key))
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
                }
                this.keyCount++;
            }
        }

        protected void ChangeItemKey(TItem item, TKey newKey)
        {
            if (!this.ContainsItem(item))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_ItemNotExist);
            }
            TKey keyForItem = this.GetKeyForItem(item);
            if (!this.comparer.Equals(keyForItem, newKey))
            {
                if (newKey != null)
                {
                    this.AddKey(newKey, item);
                }
                if (keyForItem != null)
                {
                    this.RemoveKey(keyForItem);
                }
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            if (this.dict != null)
            {
                this.dict.Clear();
            }
            this.keyCount = 0;
        }

        public bool Contains(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            if (this.dict != null)
            {
                return this.dict.ContainsKey(key);
            }
            if (key != null)
            {
                foreach (TItem local in base.Items)
                {
                    if (this.comparer.Equals(this.GetKeyForItem(local), key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ContainsItem(TItem item)
        {
            TKey local;
            TItem local2;
            if ((this.dict == null) || ((local = this.GetKeyForItem(item)) == null))
            {
                return base.Items.Contains(item);
            }
            return (this.dict.TryGetValue(local, out local2) && EqualityComparer<TItem>.Default.Equals(local2, item));
        }

        private void CreateDictionary()
        {
            this.dict = new Dictionary<TKey, TItem>(this.comparer);
            foreach (TItem local in base.Items)
            {
                TKey keyForItem = this.GetKeyForItem(local);
                if (keyForItem != null)
                {
                    this.dict.Add(keyForItem, local);
                }
            }
        }

        protected abstract TKey GetKeyForItem(TItem item);
        protected override void InsertItem(int index, TItem item)
        {
            TKey keyForItem = this.GetKeyForItem(item);
            if (keyForItem != null)
            {
                this.AddKey(keyForItem, item);
            }
            base.InsertItem(index, item);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            if (this.dict != null)
            {
                return (this.dict.ContainsKey(key) && base.Remove(this.dict[key]));
            }
            if (key != null)
            {
                for (int i = 0; i < base.Items.Count; i++)
                {
                    if (this.comparer.Equals(this.GetKeyForItem(base.Items[i]), key))
                    {
                        this.RemoveItem(i);
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void RemoveItem(int index)
        {
            TKey keyForItem = this.GetKeyForItem(base.Items[index]);
            if (keyForItem != null)
            {
                this.RemoveKey(keyForItem);
            }
            base.RemoveItem(index);
        }

        private void RemoveKey(TKey key)
        {
            if (this.dict != null)
            {
                this.dict.Remove(key);
            }
            else
            {
                this.keyCount--;
            }
        }

        protected override void SetItem(int index, TItem item)
        {
            TKey keyForItem = this.GetKeyForItem(item);
            TKey x = this.GetKeyForItem(base.Items[index]);
            if (this.comparer.Equals(x, keyForItem))
            {
                if ((keyForItem != null) && (this.dict != null))
                {
                    this.dict[keyForItem] = item;
                }
            }
            else
            {
                if (keyForItem != null)
                {
                    this.AddKey(keyForItem, item);
                }
                if (x != null)
                {
                    this.RemoveKey(x);
                }
            }
            base.SetItem(index, item);
        }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return this.comparer;
            }
        }

        protected IDictionary<TKey, TItem> Dictionary
        {
            get
            {
                return this.dict;
            }
        }

        public TItem this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                }
                if (this.dict != null)
                {
                    return this.dict[key];
                }
                foreach (TItem local in base.Items)
                {
                    if (this.comparer.Equals(this.GetKeyForItem(local), key))
                    {
                        return local;
                    }
                }
                ThrowHelper.ThrowKeyNotFoundException();
                return default(TItem);
            }
        }
    }
}

