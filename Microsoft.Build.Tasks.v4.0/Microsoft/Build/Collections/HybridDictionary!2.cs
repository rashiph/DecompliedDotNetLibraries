namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    internal class HybridDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private Microsoft.Build.Collections.HybridDictionaryBackingType backingType;
        private IEqualityComparer<TKey> comparer;
        internal static readonly int MaxListSize;
        private object syncRoot;
        private Dictionary<TKey, TValue> valuesDict;
        private KeyValuePair<TKey, TValue> valueSingle;
        private List<KeyValuePair<TKey, TValue>> valuesList;

        static HybridDictionary()
        {
            Microsoft.Build.Collections.HybridDictionary<TKey, TValue>.MaxListSize = Math.Max(2, Convert.ToInt32(Environment.GetEnvironmentVariable("MSBuildHybridDictThreshold")));
        }

        public HybridDictionary() : this(0)
        {
        }

        public HybridDictionary(int capacity) : this(capacity, EqualityComparer<TKey>.Default)
        {
        }

        public HybridDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.syncRoot = new object();
            this.comparer = comparer;
            if (this.comparer == null)
            {
                this.comparer = EqualityComparer<TKey>.Default;
            }
            if (capacity > Microsoft.Build.Collections.HybridDictionary<TKey, TValue>.MaxListSize)
            {
                this.valuesDict = new Dictionary<TKey, TValue>(comparer);
                this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary;
            }
            else if (capacity > 1)
            {
                this.valuesList = new List<KeyValuePair<TKey, TValue>>(capacity);
                this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.List;
            }
            else
            {
                this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.None;
            }
        }

        public HybridDictionary(Microsoft.Build.Collections.HybridDictionary<TKey, TValue> other, IEqualityComparer<TKey> comparer) : this(other.Count, comparer)
        {
            foreach (KeyValuePair<TKey, TValue> pair in other)
            {
                this.Add(pair.Key, pair.Value);
            }
        }

        public HybridDictionary(SerializationInfo info, StreamingContext context)
        {
            this.syncRoot = new object();
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Add(TKey key, TValue value)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(key, "key");
            switch (this.backingType)
            {
                case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                    this.valueSingle = new KeyValuePair<TKey, TValue>(key, value);
                    this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.Single;
                    return;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                    if (this.comparer.Equals(this.valueSingle.Key, key))
                    {
                        throw new ArgumentException("A value with the same key is already in the collection.");
                    }
                    this.GrowForAdd();
                    this.valuesList.Add(new KeyValuePair<TKey, TValue>(key, value));
                    return;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    if (this.ContainsKey(key))
                    {
                        throw new ArgumentException("A value with the same key is already in the collection.");
                    }
                    this.GrowForAdd();
                    if (this.backingType == Microsoft.Build.Collections.HybridDictionaryBackingType.List)
                    {
                        this.valuesList.Add(new KeyValuePair<TKey, TValue>(key, value));
                        return;
                    }
                    this.valuesDict.Add(key, value);
                    return;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                    this.valuesDict.Add(key, value);
                    return;
            }
        }

        public void Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            this.valuesDict = null;
            this.valuesList = null;
            this.valueSingle = new KeyValuePair<TKey, TValue>();
            this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.None;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object key)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            TValue local;
            return this.TryGetValue(key, out local);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        private KeyValuePair<TKey, TValue>? FindEntryInList(TKey key)
        {
            foreach (KeyValuePair<TKey, TValue> pair in this.valuesList)
            {
                if (this.comparer.Equals(pair.Key, key))
                {
                    return new KeyValuePair<TKey, TValue>?(pair);
                }
            }
            return null;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            switch (this.backingType)
            {
                case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                    return Microsoft.Build.Collections.ReadOnlyEmptyCollection<KeyValuePair<TKey, TValue>>.Instance.GetEnumerator();

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                    return new SingleEnumerator<TKey, TValue>(this.valueSingle);

                case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    return this.valuesList.GetEnumerator();

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.GetEnumerator();
            }
            Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
            return null;
        }

        private void GrowForAdd()
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.backingType != Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary, "Backing type is already dictionary");
            if (this.backingType == Microsoft.Build.Collections.HybridDictionaryBackingType.List)
            {
                if (this.valuesList.Count >= Microsoft.Build.Collections.HybridDictionary<TKey, TValue>.MaxListSize)
                {
                    this.valuesDict = new Dictionary<TKey, TValue>(this.valuesList.Count * 2, this.comparer);
                    foreach (KeyValuePair<TKey, TValue> pair in this.valuesList)
                    {
                        this.valuesDict.Add(pair.Key, pair.Value);
                    }
                    this.valuesList = null;
                    this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary;
                }
            }
            else
            {
                this.valuesList = new List<KeyValuePair<TKey, TValue>>();
                this.valuesList.Add(this.valueSingle);
                this.valueSingle = new KeyValuePair<TKey, TValue>();
                this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.List;
            }
        }

        public bool Remove(TKey key)
        {
            switch (this.backingType)
            {
                case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                    return false;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                    if (!this.comparer.Equals(this.valueSingle.Key, key))
                    {
                        return false;
                    }
                    this.valueSingle = new KeyValuePair<TKey, TValue>();
                    this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.None;
                    return true;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    return this.TryRemoveFromList(key);

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.Remove(key);
            }
            Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
            return false;
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            switch (this.backingType)
            {
                case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.GetEnumerator();
            }
            Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            switch (this.backingType)
            {
                case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                    break;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                    if (!this.comparer.Equals(this.valueSingle.Key, key))
                    {
                        break;
                    }
                    value = this.valueSingle.Value;
                    return true;

                case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                {
                    KeyValuePair<TKey, TValue>? nullable = this.FindEntryInList(key);
                    if (!nullable.HasValue)
                    {
                        break;
                    }
                    value = nullable.Value.Value;
                    return true;
                }
                case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.TryGetValue(key, out value);

                default:
                    Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                    break;
            }
            value = default(TValue);
            return false;
        }

        private bool TryRemoveFromList(TKey key)
        {
            for (int i = 0; i < this.valuesList.Count; i++)
            {
                KeyValuePair<TKey, TValue> pair = this.valuesList[i];
                if (this.comparer.Equals(pair.Key, key))
                {
                    this.valuesList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        internal Microsoft.Build.Collections.HybridDictionaryBackingType BackingType
        {
            get
            {
                return this.backingType;
            }
        }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return this.comparer;
            }
        }

        public int Count
        {
            get
            {
                switch (this.backingType)
                {
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                        return 0;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                        return 1;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                        return this.valuesList.Count;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict.Count;
                }
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                return 0;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                switch (this.backingType)
                {
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                        break;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                        if (!this.comparer.Equals(this.valueSingle.Key, key))
                        {
                            break;
                        }
                        return this.valueSingle.Value;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    {
                        KeyValuePair<TKey, TValue>? nullable = this.FindEntryInList(key);
                        if (!nullable.HasValue)
                        {
                            break;
                        }
                        return nullable.Value.Value;
                    }
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict[key];

                    default:
                        Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                        break;
                }
                throw new KeyNotFoundException("The specified key was not found in the collection.");
            }
            set
            {
                switch (this.backingType)
                {
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                        this.valueSingle = new KeyValuePair<TKey, TValue>(key, value);
                        this.backingType = Microsoft.Build.Collections.HybridDictionaryBackingType.Single;
                        return;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                        if (!this.comparer.Equals(this.valueSingle.Key, key))
                        {
                            this.Add(key, value);
                            return;
                        }
                        this.valueSingle = new KeyValuePair<TKey, TValue>(key, value);
                        return;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                        this.TryRemoveFromList(key);
                        this.Add(key, value);
                        return;

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                        this.valuesDict[key] = value;
                        return;
                }
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
            }
        }

        public object this[object key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                switch (this.backingType)
                {
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                        return new TKey[0];

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                        return new TKey[] { this.valueSingle.Key };

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    {
                        TKey[] localArray = new TKey[this.valuesList.Count];
                        for (int i = 0; i < this.valuesList.Count; i++)
                        {
                            KeyValuePair<TKey, TValue> pair = this.valuesList[i];
                            localArray[i] = pair.Key;
                        }
                        return localArray;
                    }
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict.Keys;
                }
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                return null;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.syncRoot;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                switch (this.backingType)
                {
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.None:
                        return new TValue[0];

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Single:
                        return new TValue[] { this.valueSingle.Value };

                    case Microsoft.Build.Collections.HybridDictionaryBackingType.List:
                    {
                        TValue[] localArray = new TValue[this.valuesList.Count];
                        for (int i = 0; i < this.valuesList.Count; i++)
                        {
                            KeyValuePair<TKey, TValue> pair = this.valuesList[i];
                            localArray[i] = pair.Value;
                        }
                        return localArray;
                    }
                    case Microsoft.Build.Collections.HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict.Values;
                }
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                return null;
            }
        }

        private class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> baseEnumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> baseEnumerator)
            {
                this.baseEnumerator = baseEnumerator;
            }

            public bool MoveNext()
            {
                return this.baseEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.baseEnumerator.Reset();
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(this.baseEnumerator.Current.Key, this.baseEnumerator.Current.Value);
                }
            }

            public object Key
            {
                get
                {
                    return this.baseEnumerator.Current.Key;
                }
            }

            public object Value
            {
                get
                {
                    return this.baseEnumerator.Current.Value;
                }
            }
        }

        private class EmptyEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
        {
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    throw new InvalidOperationException("Past end of enumeration");
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    throw new InvalidOperationException("Past end of enumeration");
                }
            }
        }

        private class SingleEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
        {
            private bool enumerationComplete;
            private KeyValuePair<TKey, TValue> value;

            public SingleEnumerator(KeyValuePair<TKey, TValue> value)
            {
                this.value = value;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!this.enumerationComplete)
                {
                    this.enumerationComplete = true;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.enumerationComplete = false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (!this.enumerationComplete)
                    {
                        throw new InvalidOperationException("Past end of enumeration");
                    }
                    return this.value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

