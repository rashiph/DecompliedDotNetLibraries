namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal class IDictionaryContract<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> value)
        {
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> value)
        {
            return false;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
        {
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> value)
        {
            return false;
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return false;
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return false;
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get
            {
                return 0;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return default(TValue);
            }
            set
            {
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return null;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return null;
            }
        }
    }
}

