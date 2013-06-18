namespace System.Collections.Generic
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [ComVisible(false)]
    public abstract class SynchronizedKeyedCollection<K, T> : SynchronizedCollection<T>
    {
        private IEqualityComparer<K> comparer;
        private const int defaultThreshold = 0;
        private Dictionary<K, T> dictionary;
        private int keyCount;
        private int threshold;

        protected SynchronizedKeyedCollection()
        {
            this.comparer = EqualityComparer<K>.Default;
            this.threshold = 0x7fffffff;
        }

        protected SynchronizedKeyedCollection(object syncRoot) : base(syncRoot)
        {
            this.comparer = EqualityComparer<K>.Default;
            this.threshold = 0x7fffffff;
        }

        protected SynchronizedKeyedCollection(object syncRoot, IEqualityComparer<K> comparer) : base(syncRoot)
        {
            if (comparer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("comparer"));
            }
            this.comparer = comparer;
            this.threshold = 0x7fffffff;
        }

        protected SynchronizedKeyedCollection(object syncRoot, IEqualityComparer<K> comparer, int dictionaryCreationThreshold) : base(syncRoot)
        {
            if (comparer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("comparer"));
            }
            if (dictionaryCreationThreshold < -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("dictionaryCreationThreshold", dictionaryCreationThreshold, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { -1, 0x7fffffff })));
            }
            if (dictionaryCreationThreshold == -1)
            {
                this.threshold = 0x7fffffff;
            }
            else
            {
                this.threshold = dictionaryCreationThreshold;
            }
            this.comparer = comparer;
        }

        private void AddKey(K key, T item)
        {
            if (this.dictionary != null)
            {
                this.dictionary.Add(key, item);
            }
            else if (this.keyCount == this.threshold)
            {
                this.CreateDictionary();
                this.dictionary.Add(key, item);
            }
            else
            {
                if (this.Contains(key))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("CannotAddTwoItemsWithTheSameKeyToSynchronizedKeyedCollection0")));
                }
                this.keyCount++;
            }
        }

        protected void ChangeItemKey(T item, K newKey)
        {
            if (!this.ContainsItem(item))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("ItemDoesNotExistInSynchronizedKeyedCollection0")));
            }
            K keyForItem = this.GetKeyForItem(item);
            if (!this.comparer.Equals(newKey, keyForItem))
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
            if (this.dictionary != null)
            {
                this.dictionary.Clear();
            }
            this.keyCount = 0;
        }

        public bool Contains(K key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
            }
            lock (base.SyncRoot)
            {
                if (this.dictionary != null)
                {
                    return this.dictionary.ContainsKey(key);
                }
                if (key != null)
                {
                    for (int i = 0; i < base.Items.Count; i++)
                    {
                        T item = base.Items[i];
                        if (this.comparer.Equals(key, this.GetKeyForItem(item)))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private bool ContainsItem(T item)
        {
            K local;
            T local2;
            if ((this.dictionary == null) || ((local = this.GetKeyForItem(item)) == null))
            {
                return base.Items.Contains(item);
            }
            return (this.dictionary.TryGetValue(local, out local2) && EqualityComparer<T>.Default.Equals(item, local2));
        }

        private void CreateDictionary()
        {
            this.dictionary = new Dictionary<K, T>(this.comparer);
            foreach (T local in base.Items)
            {
                K keyForItem = this.GetKeyForItem(local);
                if (keyForItem != null)
                {
                    this.dictionary.Add(keyForItem, local);
                }
            }
        }

        protected abstract K GetKeyForItem(T item);
        protected override void InsertItem(int index, T item)
        {
            K keyForItem = this.GetKeyForItem(item);
            if (keyForItem != null)
            {
                this.AddKey(keyForItem, item);
            }
            base.InsertItem(index, item);
        }

        public bool Remove(K key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
            }
            lock (base.SyncRoot)
            {
                if (this.dictionary != null)
                {
                    return (this.dictionary.ContainsKey(key) && base.Remove(this.dictionary[key]));
                }
                for (int i = 0; i < base.Items.Count; i++)
                {
                    if (this.comparer.Equals(key, this.GetKeyForItem(base.Items[i])))
                    {
                        this.RemoveItem(i);
                        return true;
                    }
                }
                return false;
            }
        }

        protected override void RemoveItem(int index)
        {
            K keyForItem = this.GetKeyForItem(base.Items[index]);
            if (keyForItem != null)
            {
                this.RemoveKey(keyForItem);
            }
            base.RemoveItem(index);
        }

        private void RemoveKey(K key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            if (this.dictionary != null)
            {
                this.dictionary.Remove(key);
            }
            else
            {
                this.keyCount--;
            }
        }

        protected override void SetItem(int index, T item)
        {
            K keyForItem = this.GetKeyForItem(item);
            K y = this.GetKeyForItem(base.Items[index]);
            if (this.comparer.Equals(keyForItem, y))
            {
                if ((keyForItem != null) && (this.dictionary != null))
                {
                    this.dictionary[keyForItem] = item;
                }
            }
            else
            {
                if (keyForItem != null)
                {
                    this.AddKey(keyForItem, item);
                }
                if (y != null)
                {
                    this.RemoveKey(y);
                }
            }
            base.SetItem(index, item);
        }

        protected IDictionary<K, T> Dictionary
        {
            get
            {
                return this.dictionary;
            }
        }

        public T this[K key]
        {
            get
            {
                T local2;
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));
                }
                lock (base.SyncRoot)
                {
                    if (this.dictionary != null)
                    {
                        local2 = this.dictionary[key];
                    }
                    else
                    {
                        for (int i = 0; i < base.Items.Count; i++)
                        {
                            T item = base.Items[i];
                            if (this.comparer.Equals(key, this.GetKeyForItem(item)))
                            {
                                return item;
                            }
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new KeyNotFoundException());
                    }
                }
                return local2;
            }
        }
    }
}

