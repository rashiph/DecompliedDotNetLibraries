namespace System.Runtime.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private OrderedDictionary privateDictionary;

        public OrderedDictionary()
        {
            this.privateDictionary = new OrderedDictionary();
        }

        public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                this.privateDictionary = new OrderedDictionary();
                foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                {
                    this.privateDictionary.Add(pair.Key, pair.Value);
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }
            this.privateDictionary.Add(key, value);
        }

        public void Clear()
        {
            this.privateDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return (((item.Key != null) && this.privateDictionary.Contains(item.Key)) && this.privateDictionary[item.Key].Equals(item.Value));
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }
            return this.privateDictionary.Contains(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw Fx.Exception.ArgumentNull("array");
            }
            if (arrayIndex < 0)
            {
                throw Fx.Exception.AsError(new ArgumentOutOfRangeException("arrayIndex"));
            }
            if (((array.Rank > 1) || (arrayIndex >= array.Length)) || ((array.Length - arrayIndex) < this.privateDictionary.Count))
            {
                throw Fx.Exception.Argument("array", SRCore.BadCopyToArray);
            }
            int index = arrayIndex;
            foreach (DictionaryEntry entry in this.privateDictionary)
            {
                array[index] = new KeyValuePair<TKey, TValue>((TKey) entry.Key, (TValue) entry.Value);
                index++;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IDictionaryEnumerator enumerator = this.privateDictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                yield return new KeyValuePair<TKey, TValue>((TKey) current.Key, (TValue) current.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this.Contains(item))
            {
                this.privateDictionary.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }
            if (this.privateDictionary.Contains(key))
            {
                this.privateDictionary.Remove(key);
                return true;
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw Fx.Exception.ArgumentNull("key");
            }
            bool flag = this.privateDictionary.Contains(key);
            value = flag ? ((TValue) this.privateDictionary[key]) : default(TValue);
            return flag;
        }

        public int Count
        {
            get
            {
                return this.privateDictionary.Count;
            }
        }

        public bool IsReadOnly
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
                if (key == null)
                {
                    throw Fx.Exception.ArgumentNull("key");
                }
                if (!this.privateDictionary.Contains(key))
                {
                    throw Fx.Exception.AsError(new KeyNotFoundException(SRCore.KeyNotFoundInDictionary));
                }
                return (TValue) this.privateDictionary[key];
            }
            set
            {
                if (key == null)
                {
                    throw Fx.Exception.ArgumentNull("key");
                }
                this.privateDictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> list = new List<TKey>(this.privateDictionary.Count);
                foreach (TKey local in this.privateDictionary.Keys)
                {
                    list.Add(local);
                }
                return list;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> list = new List<TValue>(this.privateDictionary.Count);
                foreach (TValue local in this.privateDictionary.Values)
                {
                    list.Add(local);
                }
                return list;
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private KeyValuePair<TKey, TValue> <>2__current;
            public OrderedDictionary<TKey, TValue> <>4__this;
            public IDictionaryEnumerator <>7__wrap2;
            public IDisposable <>7__wrap3;
            public DictionaryEntry <entry>5__1;

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public <GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private void <>m__Finally4()
            {
                this.<>1__state = -1;
                this.<>7__wrap3 = this.<>7__wrap2 as IDisposable;
                if (this.<>7__wrap3 != null)
                {
                    this.<>7__wrap3.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap2 = this.<>4__this.privateDictionary.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_009D;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_009D;

                        default:
                            goto Label_00B0;
                    }
                Label_0044:
                    this.<entry>5__1 = (DictionaryEntry) this.<>7__wrap2.Current;
                    this.<>2__current = new KeyValuePair<TKey, TValue>((TKey) this.<entry>5__1.Key, (TValue) this.<entry>5__1.Value);
                    this.<>1__state = 2;
                    return true;
                Label_009D:
                    if (this.<>7__wrap2.MoveNext())
                    {
                        goto Label_0044;
                    }
                    this.<>m__Finally4();
                Label_00B0:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case 1:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally4();
                        }
                        return;
                }
            }

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

