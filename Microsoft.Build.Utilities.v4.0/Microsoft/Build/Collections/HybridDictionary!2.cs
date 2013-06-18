namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    internal class HybridDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private HybridDictionaryBackingType backingType;
        private IEqualityComparer<TKey> comparer;
        internal static readonly int MaxListSize;
        private object syncRoot;
        private Dictionary<TKey, TValue> valuesDict;
        private KeyValuePair<TKey, TValue> valueSingle;
        private List<KeyValuePair<TKey, TValue>> valuesList;

        static HybridDictionary()
        {
            HybridDictionary<TKey, TValue>.MaxListSize = Math.Max(2, Convert.ToInt32(Environment.GetEnvironmentVariable("MSBuildHybridDictThreshold")));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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
            if (capacity > HybridDictionary<TKey, TValue>.MaxListSize)
            {
                this.valuesDict = new Dictionary<TKey, TValue>(comparer);
                this.backingType = HybridDictionaryBackingType.Dictionary;
            }
            else if (capacity > 1)
            {
                this.valuesList = new List<KeyValuePair<TKey, TValue>>(capacity);
                this.backingType = HybridDictionaryBackingType.List;
            }
            else
            {
                this.backingType = HybridDictionaryBackingType.None;
            }
        }

        public HybridDictionary(HybridDictionary<TKey, TValue> other, IEqualityComparer<TKey> comparer) : this(other.Count, comparer)
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
            ErrorUtilities.VerifyThrowArgumentNull(key, "key");
            switch (this.backingType)
            {
                case HybridDictionaryBackingType.None:
                    this.valueSingle = new KeyValuePair<TKey, TValue>(key, value);
                    this.backingType = HybridDictionaryBackingType.Single;
                    return;

                case HybridDictionaryBackingType.Single:
                    if (this.comparer.Equals(this.valueSingle.Key, key))
                    {
                        throw new ArgumentException("A value with the same key is already in the collection.");
                    }
                    this.GrowForAdd();
                    this.valuesList.Add(new KeyValuePair<TKey, TValue>(key, value));
                    return;

                case HybridDictionaryBackingType.List:
                    if (this.ContainsKey(key))
                    {
                        throw new ArgumentException("A value with the same key is already in the collection.");
                    }
                    this.GrowForAdd();
                    if (this.backingType == HybridDictionaryBackingType.List)
                    {
                        this.valuesList.Add(new KeyValuePair<TKey, TValue>(key, value));
                        return;
                    }
                    this.valuesDict.Add(key, value);
                    return;

                case HybridDictionaryBackingType.Dictionary:
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
            this.backingType = HybridDictionaryBackingType.None;
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
                case HybridDictionaryBackingType.None:
                    return ReadOnlyEmptyCollection<KeyValuePair<TKey, TValue>>.Instance.GetEnumerator();

                case HybridDictionaryBackingType.Single:
                    return new SingleEnumerator<TKey, TValue>(this.valueSingle);

                case HybridDictionaryBackingType.List:
                    return this.valuesList.GetEnumerator();

                case HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.GetEnumerator();
            }
            ErrorUtilities.ThrowInternalErrorUnreachable();
            return null;
        }

        private void GrowForAdd()
        {
            ErrorUtilities.VerifyThrow(this.backingType != HybridDictionaryBackingType.Dictionary, "Backing type is already dictionary");
            if (this.backingType == HybridDictionaryBackingType.List)
            {
                if (this.valuesList.Count >= HybridDictionary<TKey, TValue>.MaxListSize)
                {
                    this.valuesDict = new Dictionary<TKey, TValue>(this.valuesList.Count * 2, this.comparer);
                    foreach (KeyValuePair<TKey, TValue> pair in this.valuesList)
                    {
                        this.valuesDict.Add(pair.Key, pair.Value);
                    }
                    this.valuesList = null;
                    this.backingType = HybridDictionaryBackingType.Dictionary;
                }
            }
            else
            {
                this.valuesList = new List<KeyValuePair<TKey, TValue>>();
                this.valuesList.Add(this.valueSingle);
                this.valueSingle = new KeyValuePair<TKey, TValue>();
                this.backingType = HybridDictionaryBackingType.List;
            }
        }

        public bool Remove(TKey key)
        {
            switch (this.backingType)
            {
                case HybridDictionaryBackingType.None:
                    return false;

                case HybridDictionaryBackingType.Single:
                    if (!this.comparer.Equals(this.valueSingle.Key, key))
                    {
                        return false;
                    }
                    this.valueSingle = new KeyValuePair<TKey, TValue>();
                    this.backingType = HybridDictionaryBackingType.None;
                    return true;

                case HybridDictionaryBackingType.List:
                    return this.TryRemoveFromList(key);

                case HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.Remove(key);
            }
            ErrorUtilities.ThrowInternalErrorUnreachable();
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
                case HybridDictionaryBackingType.None:
                case HybridDictionaryBackingType.Single:
                case HybridDictionaryBackingType.List:
                    return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());

                case HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.GetEnumerator();
            }
            ErrorUtilities.ThrowInternalErrorUnreachable();
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            switch (this.backingType)
            {
                case HybridDictionaryBackingType.None:
                    break;

                case HybridDictionaryBackingType.Single:
                    if (!this.comparer.Equals(this.valueSingle.Key, key))
                    {
                        break;
                    }
                    value = this.valueSingle.Value;
                    return true;

                case HybridDictionaryBackingType.List:
                {
                    KeyValuePair<TKey, TValue>? nullable = this.FindEntryInList(key);
                    if (!nullable.HasValue)
                    {
                        break;
                    }
                    value = nullable.Value.Value;
                    return true;
                }
                case HybridDictionaryBackingType.Dictionary:
                    return this.valuesDict.TryGetValue(key, out value);

                default:
                    ErrorUtilities.ThrowInternalErrorUnreachable();
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

        internal HybridDictionaryBackingType BackingType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.backingType;
            }
        }

        public IEqualityComparer<TKey> Comparer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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
                    case HybridDictionaryBackingType.None:
                        return 0;

                    case HybridDictionaryBackingType.Single:
                        return 1;

                    case HybridDictionaryBackingType.List:
                        return this.valuesList.Count;

                    case HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict.Count;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
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
                    case HybridDictionaryBackingType.None:
                        break;

                    case HybridDictionaryBackingType.Single:
                        if (!this.comparer.Equals(this.valueSingle.Key, key))
                        {
                            break;
                        }
                        return this.valueSingle.Value;

                    case HybridDictionaryBackingType.List:
                    {
                        KeyValuePair<TKey, TValue>? nullable = this.FindEntryInList(key);
                        if (!nullable.HasValue)
                        {
                            break;
                        }
                        return nullable.Value.Value;
                    }
                    case HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict[key];

                    default:
                        ErrorUtilities.ThrowInternalErrorUnreachable();
                        break;
                }
                throw new KeyNotFoundException("The specified key was not found in the collection.");
            }
            set
            {
                switch (this.backingType)
                {
                    case HybridDictionaryBackingType.None:
                        this.valueSingle = new KeyValuePair<TKey, TValue>(key, value);
                        this.backingType = HybridDictionaryBackingType.Single;
                        return;

                    case HybridDictionaryBackingType.Single:
                        if (!this.comparer.Equals(this.valueSingle.Key, key))
                        {
                            this.Add(key, value);
                            return;
                        }
                        this.valueSingle = new KeyValuePair<TKey, TValue>(key, value);
                        return;

                    case HybridDictionaryBackingType.List:
                        this.TryRemoveFromList(key);
                        this.Add(key, value);
                        return;

                    case HybridDictionaryBackingType.Dictionary:
                        this.valuesDict[key] = value;
                        return;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
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
                    case HybridDictionaryBackingType.None:
                        return new TKey[0];

                    case HybridDictionaryBackingType.Single:
                        return new TKey[] { this.valueSingle.Key };

                    case HybridDictionaryBackingType.List:
                    {
                        TKey[] localArray = new TKey[this.valuesList.Count];
                        for (int i = 0; i < this.valuesList.Count; i++)
                        {
                            KeyValuePair<TKey, TValue> pair = this.valuesList[i];
                            localArray[i] = pair.Key;
                        }
                        return localArray;
                    }
                    case HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict.Keys;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
                return null;
            }
        }

        public object SyncRoot
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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
                    case HybridDictionaryBackingType.None:
                        return new TValue[0];

                    case HybridDictionaryBackingType.Single:
                        return new TValue[] { this.valueSingle.Value };

                    case HybridDictionaryBackingType.List:
                    {
                        TValue[] localArray = new TValue[this.valuesList.Count];
                        for (int i = 0; i < this.valuesList.Count; i++)
                        {
                            KeyValuePair<TKey, TValue> pair = this.valuesList[i];
                            localArray[i] = pair.Value;
                        }
                        return localArray;
                    }
                    case HybridDictionaryBackingType.Dictionary:
                        return this.valuesDict.Values;
                }
                ErrorUtilities.ThrowInternalErrorUnreachable();
                return null;
            }
        }

        private class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> baseEnumerator;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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

