namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, DebuggerTypeProxy(typeof(System_DictionaryDebugView<,>)), DebuggerDisplay("Count = {Count}")]
    public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private TreeSet<KeyValuePair<TKey, TValue>> _set;
        [NonSerialized]
        private KeyCollection<TKey, TValue> keys;
        [NonSerialized]
        private ValueCollection<TKey, TValue> values;

        public SortedDictionary() : this((IComparer<TKey>) null)
        {
        }

        public SortedDictionary(IComparer<TKey> comparer)
        {
            this._set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer<TKey, TValue>(comparer));
        }

        public SortedDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
        {
        }

        public SortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            if (dictionary == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.dictionary);
            }
            this._set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer<TKey, TValue>(comparer));
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                this._set.Add(pair);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
            }
            this._set.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Clear()
        {
            this._set.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
            }
            return this._set.Contains(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        public bool ContainsValue(TValue value)
        {
            TreeWalkPredicate<KeyValuePair<TKey, TValue>> action = null;
            bool found = false;
            if (value == null)
            {
                if (action == null)
                {
                    action = delegate (SortedSet<KeyValuePair<TKey, TValue>>.Node node) {
                        if (node.Item.Value == null)
                        {
                            found = true;
                            return false;
                        }
                        return true;
                    };
                }
                this._set.InOrderTreeWalk(action);
            }
            else
            {
                EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
                this._set.InOrderTreeWalk(delegate (SortedSet<KeyValuePair<TKey, TValue>>.Node node) {
                    if (valueComparer.Equals(node.Item.Value, value))
                    {
                        found = true;
                        return false;
                    }
                    return true;
                });
            }
            return found;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            this._set.CopyTo(array, index);
        }

        public Enumerator<TKey, TValue> GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((SortedDictionary<TKey, TValue>) this, 1);
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
            }
            return (key is TKey);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
            }
            return this._set.Remove(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            this._set.Add(keyValuePair);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            SortedSet<KeyValuePair<TKey, TValue>>.Node node = this._set.FindNode(keyValuePair);
            if (node == null)
            {
                return false;
            }
            if (keyValuePair.Value == null)
            {
                return (node.Item.Value == null);
            }
            return EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            SortedSet<KeyValuePair<TKey, TValue>>.Node node = this._set.FindNode(keyValuePair);
            if ((node != null) && EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value))
            {
                this._set.Remove(keyValuePair);
                return true;
            }
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((SortedDictionary<TKey, TValue>) this, 1);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this._set.CopyTo(array, index);
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
            }
            System.ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, System.ExceptionArgument.value);
            try
            {
                TKey local = (TKey) key;
                try
                {
                    this.Add(local, (TValue) value);
                }
                catch (InvalidCastException)
                {
                    System.ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                }
            }
            catch (InvalidCastException)
            {
                System.ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
            }
        }

        bool IDictionary.Contains(object key)
        {
            return (SortedDictionary<TKey, TValue>.IsCompatibleKey(key) && this.ContainsKey((TKey) key));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((SortedDictionary<TKey, TValue>) this, 2);
        }

        void IDictionary.Remove(object key)
        {
            if (SortedDictionary<TKey, TValue>.IsCompatibleKey(key))
            {
                this.Remove((TKey) key);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((SortedDictionary<TKey, TValue>) this, 1);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
            }
            SortedSet<KeyValuePair<TKey, TValue>>.Node node = this._set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
            if (node == null)
            {
                value = default(TValue);
                return false;
            }
            value = node.Item.Value;
            return true;
        }

        public IComparer<TKey> Comparer
        {
            get
            {
                return ((KeyValuePairComparer<TKey, TValue>) this._set.Comparer).keyComparer;
            }
        }

        public int Count
        {
            get
            {
                return this._set.Count;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
                }
                SortedSet<KeyValuePair<TKey, TValue>>.Node node = this._set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
                if (node == null)
                {
                    System.ThrowHelper.ThrowKeyNotFoundException();
                }
                return node.Item.Value;
            }
            set
            {
                if (key == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
                }
                SortedSet<KeyValuePair<TKey, TValue>>.Node node = this._set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
                if (node == null)
                {
                    this._set.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    node.Item = new KeyValuePair<TKey, TValue>(node.Item.Key, value);
                    this._set.UpdateVersion();
                }
            }
        }

        public KeyCollection<TKey, TValue> Keys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new KeyCollection<TKey, TValue>((SortedDictionary<TKey, TValue>) this);
                }
                return this.keys;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return this.Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return this.Values;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this._set.SyncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                TValue local;
                if (SortedDictionary<TKey, TValue>.IsCompatibleKey(key) && this.TryGetValue((TKey) key, out local))
                {
                    return local;
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.key);
                }
                System.ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, System.ExceptionArgument.value);
                try
                {
                    TKey local = (TKey) key;
                    try
                    {
                        this[local] = (TValue) value;
                    }
                    catch (InvalidCastException)
                    {
                        System.ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                    }
                }
                catch (InvalidCastException)
                {
                    System.ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return this.Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return this.Values;
            }
        }

        public ValueCollection<TKey, TValue> Values
        {
            get
            {
                if (this.values == null)
                {
                    this.values = new ValueCollection<TKey, TValue>((SortedDictionary<TKey, TValue>) this);
                }
                return this.values;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
        {
            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;
            private SortedSet<KeyValuePair<TKey, TValue>>.Enumerator treeEnum;
            private int getEnumeratorRetType;
            internal Enumerator(SortedDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.treeEnum = dictionary._set.GetEnumerator();
                this.getEnumeratorRetType = getEnumeratorRetType;
            }

            public bool MoveNext()
            {
                return this.treeEnum.MoveNext();
            }

            public void Dispose()
            {
                this.treeEnum.Dispose();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return this.treeEnum.Current;
                }
            }
            internal bool NotStartedOrEnded
            {
                get
                {
                    return this.treeEnum.NotStartedOrEnded;
                }
            }
            internal void Reset()
            {
                this.treeEnum.Reset();
            }

            void IEnumerator.Reset()
            {
                this.treeEnum.Reset();
            }

            object IEnumerator.Current
            {
                get
                {
                    if (this.NotStartedOrEnded)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    if (this.getEnumeratorRetType == 2)
                    {
                        return new DictionaryEntry(this.Current.Key, this.Current.Value);
                    }
                    return new KeyValuePair<TKey, TValue>(this.Current.Key, this.Current.Value);
                }
            }
            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (this.NotStartedOrEnded)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.Current.Key;
                }
            }
            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (this.NotStartedOrEnded)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.Current.Value;
                }
            }
            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (this.NotStartedOrEnded)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return new DictionaryEntry(this.Current.Key, this.Current.Value);
                }
            }
        }

        [Serializable, DebuggerTypeProxy(typeof(System_DictionaryKeyCollectionDebugView<,>)), DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
        {
            private SortedDictionary<TKey, TValue> dictionary;

            public KeyCollection(SortedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
                }
                if (index < 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.index);
                }
                if ((array.Length - index) < this.Count)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                this.dictionary._set.InOrderTreeWalk(delegate (SortedSet<KeyValuePair<TKey, TValue>>.Node node) {
                    array[index++] = node.Item.Key;
                    return true;
                });
            }

            public Enumerator<TKey, TValue> GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection<TKey>.Add(TKey item)
            {
                System.ThrowHelper.ThrowNotSupportedException(System.ExceptionResource.NotSupported_KeyCollectionSet);
            }

            void ICollection<TKey>.Clear()
            {
                System.ThrowHelper.ThrowNotSupportedException(System.ExceptionResource.NotSupported_KeyCollectionSet);
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return this.dictionary.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                System.ThrowHelper.ThrowNotSupportedException(System.ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
                }
                if (array.Rank != 1)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_RankMultiDimNotSupported);
                }
                if (array.GetLowerBound(0) != 0)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_NonZeroLowerBound);
                }
                if (index < 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.arrayIndex, System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((array.Length - index) < this.dictionary.Count)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                TKey[] localArray = array as TKey[];
                if (localArray != null)
                {
                    this.CopyTo(localArray, index);
                }
                else
                {
                    TreeWalkPredicate<KeyValuePair<TKey, TValue>> action = null;
                    object[] objects = (object[]) array;
                    if (objects == null)
                    {
                        System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
                    }
                    try
                    {
                        if (action == null)
                        {
                            action = delegate (SortedSet<KeyValuePair<TKey, TValue>>.Node node) {
                                objects[index++] = node.Item.Key;
                                return true;
                            };
                        }
                        this.dictionary._set.InOrderTreeWalk(action);
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            public int Count
            {
                get
                {
                    return this.dictionary.Count;
                }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return ((ICollection) this.dictionary).SyncRoot;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
            {
                private SortedDictionary<TKey, TValue>.Enumerator dictEnum;
                internal Enumerator(SortedDictionary<TKey, TValue> dictionary)
                {
                    this.dictEnum = dictionary.GetEnumerator();
                }

                public void Dispose()
                {
                    this.dictEnum.Dispose();
                }

                public bool MoveNext()
                {
                    return this.dictEnum.MoveNext();
                }

                public TKey Current
                {
                    get
                    {
                        return this.dictEnum.Current.Key;
                    }
                }
                object IEnumerator.Current
                {
                    get
                    {
                        if (this.dictEnum.NotStartedOrEnded)
                        {
                            System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        }
                        return this.Current;
                    }
                }
                void IEnumerator.Reset()
                {
                    this.dictEnum.Reset();
                }
            }
        }

        [Serializable]
        internal class KeyValuePairComparer : Comparer<KeyValuePair<TKey, TValue>>
        {
            internal IComparer<TKey> keyComparer;

            public KeyValuePairComparer(IComparer<TKey> keyComparer)
            {
                if (keyComparer == null)
                {
                    this.keyComparer = Comparer<TKey>.Default;
                }
                else
                {
                    this.keyComparer = keyComparer;
                }
            }

            public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return this.keyComparer.Compare(x.Key, y.Key);
            }
        }

        [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(System_DictionaryValueCollectionDebugView<,>))]
        public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
        {
            private SortedDictionary<TKey, TValue> dictionary;

            public ValueCollection(SortedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
                }
                if (index < 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.index);
                }
                if ((array.Length - index) < this.Count)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                this.dictionary._set.InOrderTreeWalk(delegate (SortedSet<KeyValuePair<TKey, TValue>>.Node node) {
                    array[index++] = node.Item.Value;
                    return true;
                });
            }

            public Enumerator<TKey, TValue> GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection<TValue>.Add(TValue item)
            {
                System.ThrowHelper.ThrowNotSupportedException(System.ExceptionResource.NotSupported_ValueCollectionSet);
            }

            void ICollection<TValue>.Clear()
            {
                System.ThrowHelper.ThrowNotSupportedException(System.ExceptionResource.NotSupported_ValueCollectionSet);
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return this.dictionary.ContainsValue(item);
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                System.ThrowHelper.ThrowNotSupportedException(System.ExceptionResource.NotSupported_ValueCollectionSet);
                return false;
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
                }
                if (array.Rank != 1)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_RankMultiDimNotSupported);
                }
                if (array.GetLowerBound(0) != 0)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_NonZeroLowerBound);
                }
                if (index < 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.arrayIndex, System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((array.Length - index) < this.dictionary.Count)
                {
                    System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                TValue[] localArray = array as TValue[];
                if (localArray != null)
                {
                    this.CopyTo(localArray, index);
                }
                else
                {
                    TreeWalkPredicate<KeyValuePair<TKey, TValue>> action = null;
                    object[] objects = (object[]) array;
                    if (objects == null)
                    {
                        System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
                    }
                    try
                    {
                        if (action == null)
                        {
                            action = delegate (SortedSet<KeyValuePair<TKey, TValue>>.Node node) {
                                objects[index++] = node.Item.Value;
                                return true;
                            };
                        }
                        this.dictionary._set.InOrderTreeWalk(action);
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            public int Count
            {
                get
                {
                    return this.dictionary.Count;
                }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return ((ICollection) this.dictionary).SyncRoot;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
            {
                private SortedDictionary<TKey, TValue>.Enumerator dictEnum;
                internal Enumerator(SortedDictionary<TKey, TValue> dictionary)
                {
                    this.dictEnum = dictionary.GetEnumerator();
                }

                public void Dispose()
                {
                    this.dictEnum.Dispose();
                }

                public bool MoveNext()
                {
                    return this.dictEnum.MoveNext();
                }

                public TValue Current
                {
                    get
                    {
                        return this.dictEnum.Current.Value;
                    }
                }
                object IEnumerator.Current
                {
                    get
                    {
                        if (this.dictEnum.NotStartedOrEnded)
                        {
                            System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        }
                        return this.Current;
                    }
                }
                void IEnumerator.Reset()
                {
                    this.dictEnum.Reset();
                }
            }
        }
    }
}

