namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class CasesDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private IDictionary<TKey, TValue> innerDictionary;
        private bool isNullKeyPresent;
        private TValue nullKeyValue;

        public CasesDictionary()
        {
            this.innerDictionary = new Dictionary<TKey, TValue>();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                if (this.isNullKeyPresent)
                {
                    throw FxTrace.Exception.Argument("key", System.Activities.SR.NullKeyAlreadyPresent);
                }
                this.isNullKeyPresent = true;
                this.nullKeyValue = value;
            }
            else
            {
                this.innerDictionary.Add(key, value);
            }
        }

        public void Clear()
        {
            this.isNullKeyPresent = false;
            this.nullKeyValue = default(TValue);
            this.innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key != null)
            {
                return this.innerDictionary.Contains(item);
            }
            if (!this.isNullKeyPresent)
            {
                return false;
            }
            if (item.Value != null)
            {
                return item.Value.Equals(this.nullKeyValue);
            }
            return (this.nullKeyValue == null);
        }

        public bool ContainsKey(TKey key)
        {
            if (key != null)
            {
                return this.innerDictionary.ContainsKey(key);
            }
            return this.isNullKeyPresent;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.innerDictionary.CopyTo(array, arrayIndex);
            if (this.isNullKeyPresent)
            {
                array[arrayIndex + this.innerDictionary.Count] = new KeyValuePair<TKey, TValue>(default(TKey), this.nullKeyValue);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<TKey, TValue>> enumerator = this.innerDictionary.GetEnumerator();
        Label_PostSwitchInIterator:;
            if (enumerator.MoveNext())
            {
                yield return enumerator.Current;
                goto Label_PostSwitchInIterator;
            }
            if (this.isNullKeyPresent)
            {
                yield return new KeyValuePair<TKey, TValue>(default(TKey), this.nullKeyValue);
            }
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                bool isNullKeyPresent = this.isNullKeyPresent;
                this.isNullKeyPresent = false;
                this.nullKeyValue = default(TValue);
                return isNullKeyPresent;
            }
            return this.innerDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key != null)
            {
                return this.innerDictionary.Remove(item);
            }
            if (this.Contains(item))
            {
                this.isNullKeyPresent = false;
                this.nullKeyValue = default(TValue);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key != null)
            {
                return this.innerDictionary.TryGetValue(key, out value);
            }
            if (this.isNullKeyPresent)
            {
                value = this.nullKeyValue;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public int Count
        {
            get
            {
                return (this.innerDictionary.Count + (this.isNullKeyPresent ? 1 : 0));
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
                if (key != null)
                {
                    return this.innerDictionary[key];
                }
                if (!this.isNullKeyPresent)
                {
                    throw FxTrace.Exception.AsError(new KeyNotFoundException());
                }
                return this.nullKeyValue;
            }
            set
            {
                if (key == null)
                {
                    this.isNullKeyPresent = true;
                    this.nullKeyValue = value;
                }
                else
                {
                    this.innerDictionary[key] = value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return new NullKeyDictionaryKeyCollection<TKey, TValue, TKey, TValue>((CasesDictionary<TKey, TValue>) this);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return new NullKeyDictionaryValueCollection<TKey, TValue, TKey, TValue>((CasesDictionary<TKey, TValue>) this);
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private KeyValuePair<TKey, TValue> <>2__current;
            public CasesDictionary<TKey, TValue> <>4__this;
            public IEnumerator<KeyValuePair<TKey, TValue>> <innerEnumerator>5__1;

            [DebuggerHidden]
            public <GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<innerEnumerator>5__1 = this.<>4__this.innerDictionary.GetEnumerator();
                        break;

                    case 1:
                        this.<>1__state = -1;
                        break;

                    case 2:
                        this.<>1__state = -1;
                        goto Label_00A7;

                    default:
                        goto Label_00A7;
                }
                if (this.<innerEnumerator>5__1.MoveNext())
                {
                    this.<>2__current = this.<innerEnumerator>5__1.Current;
                    this.<>1__state = 1;
                    return true;
                }
                if (this.<>4__this.isNullKeyPresent)
                {
                    this.<>2__current = new KeyValuePair<TKey, TValue>(default(TKey), this.<>4__this.nullKeyValue);
                    this.<>1__state = 2;
                    return true;
                }
            Label_00A7:
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
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

        private class NullKeyDictionaryKeyCollection<TypeKey, TypeValue> : ICollection<TypeKey>, IEnumerable<TypeKey>, IEnumerable
        {
            private CasesDictionary<TypeKey, TypeValue> nullKeyDictionary;

            public NullKeyDictionaryKeyCollection(CasesDictionary<TypeKey, TypeValue> nullKeyDictionary)
            {
                this.nullKeyDictionary = nullKeyDictionary;
            }

            public void Add(TypeKey item)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.KeyCollectionUpdatesNotAllowed));
            }

            public void Clear()
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.KeyCollectionUpdatesNotAllowed));
            }

            public bool Contains(TypeKey item)
            {
                if (item != null)
                {
                    return this.nullKeyDictionary.innerDictionary.Keys.Contains(item);
                }
                return this.nullKeyDictionary.isNullKeyPresent;
            }

            public void CopyTo(TypeKey[] array, int arrayIndex)
            {
                this.nullKeyDictionary.innerDictionary.Keys.CopyTo(array, arrayIndex);
                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    array[arrayIndex + this.nullKeyDictionary.innerDictionary.Keys.Count] = default(TypeKey);
                }
            }

            public IEnumerator<TypeKey> GetEnumerator()
            {
                foreach (TypeKey iteratorVariable0 in this.nullKeyDictionary.innerDictionary.Keys)
                {
                    yield return iteratorVariable0;
                }
                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    yield return default(TypeKey);
                }
            }

            public bool Remove(TypeKey item)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.KeyCollectionUpdatesNotAllowed));
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    int count = this.nullKeyDictionary.innerDictionary.Keys.Count;
                    if (this.nullKeyDictionary.isNullKeyPresent)
                    {
                        count++;
                    }
                    return count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            [CompilerGenerated]
            private sealed class <GetEnumerator>d__3 : IEnumerator<TypeKey>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private TypeKey <>2__current;
                public CasesDictionary<TKey, TValue>.NullKeyDictionaryKeyCollection<TypeKey, TypeValue> <>4__this;
                public IEnumerator<TypeKey> <>7__wrap5;
                public TypeKey <item>5__4;

                [DebuggerHidden]
                public <GetEnumerator>d__3(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                }

                private void <>m__Finally6()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap5 != null)
                    {
                        this.<>7__wrap5.Dispose();
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
                                this.<>7__wrap5 = this.<>4__this.nullKeyDictionary.innerDictionary.Keys.GetEnumerator();
                                this.<>1__state = 1;
                                goto Label_0081;

                            case 2:
                                this.<>1__state = 1;
                                goto Label_0081;

                            case 3:
                                this.<>1__state = -1;
                                goto Label_00C7;

                            default:
                                goto Label_00C7;
                        }
                    Label_0052:
                        this.<item>5__4 = this.<>7__wrap5.Current;
                        this.<>2__current = this.<item>5__4;
                        this.<>1__state = 2;
                        return true;
                    Label_0081:
                        if (this.<>7__wrap5.MoveNext())
                        {
                            goto Label_0052;
                        }
                        this.<>m__Finally6();
                        if (this.<>4__this.nullKeyDictionary.isNullKeyPresent)
                        {
                            this.<>2__current = default(TypeKey);
                            this.<>1__state = 3;
                            return true;
                        }
                    Label_00C7:
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
                                this.<>m__Finally6();
                            }
                            return;
                    }
                }

                TypeKey IEnumerator<TypeKey>.Current
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

        private class NullKeyDictionaryValueCollection<TypeKey, TypeValue> : ICollection<TypeValue>, IEnumerable<TypeValue>, IEnumerable
        {
            private CasesDictionary<TypeKey, TypeValue> nullKeyDictionary;

            public NullKeyDictionaryValueCollection(CasesDictionary<TypeKey, TypeValue> nullKeyDictionary)
            {
                this.nullKeyDictionary = nullKeyDictionary;
            }

            public void Add(TypeValue item)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.ValueCollectionUpdatesNotAllowed));
            }

            public void Clear()
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.ValueCollectionUpdatesNotAllowed));
            }

            public bool Contains(TypeValue item)
            {
                return (this.nullKeyDictionary.innerDictionary.Values.Contains(item) || (this.nullKeyDictionary.isNullKeyPresent && this.nullKeyDictionary.nullKeyValue.Equals(item)));
            }

            public void CopyTo(TypeValue[] array, int arrayIndex)
            {
                this.nullKeyDictionary.innerDictionary.Values.CopyTo(array, arrayIndex);
                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    array[arrayIndex + this.nullKeyDictionary.innerDictionary.Values.Count] = this.nullKeyDictionary.nullKeyValue;
                }
            }

            public IEnumerator<TypeValue> GetEnumerator()
            {
                foreach (TypeValue iteratorVariable0 in this.nullKeyDictionary.innerDictionary.Values)
                {
                    yield return iteratorVariable0;
                }
                if (this.nullKeyDictionary.isNullKeyPresent)
                {
                    yield return this.nullKeyDictionary.nullKeyValue;
                }
            }

            public bool Remove(TypeValue item)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.ValueCollectionUpdatesNotAllowed));
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    int count = this.nullKeyDictionary.innerDictionary.Values.Count;
                    if (this.nullKeyDictionary.isNullKeyPresent)
                    {
                        count++;
                    }
                    return count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            [CompilerGenerated]
            private sealed class <GetEnumerator>d__8 : IEnumerator<TypeValue>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private TypeValue <>2__current;
                public CasesDictionary<TKey, TValue>.NullKeyDictionaryValueCollection<TypeKey, TypeValue> <>4__this;
                public IEnumerator<TypeValue> <>7__wrapa;
                public TypeValue <item>5__9;

                [DebuggerHidden]
                public <GetEnumerator>d__8(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                }

                private void <>m__Finallyb()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrapa != null)
                    {
                        this.<>7__wrapa.Dispose();
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
                                this.<>7__wrapa = this.<>4__this.nullKeyDictionary.innerDictionary.Values.GetEnumerator();
                                this.<>1__state = 1;
                                goto Label_0081;

                            case 2:
                                this.<>1__state = 1;
                                goto Label_0081;

                            case 3:
                                this.<>1__state = -1;
                                goto Label_00CE;

                            default:
                                goto Label_00CE;
                        }
                    Label_0052:
                        this.<item>5__9 = this.<>7__wrapa.Current;
                        this.<>2__current = this.<item>5__9;
                        this.<>1__state = 2;
                        return true;
                    Label_0081:
                        if (this.<>7__wrapa.MoveNext())
                        {
                            goto Label_0052;
                        }
                        this.<>m__Finallyb();
                        if (this.<>4__this.nullKeyDictionary.isNullKeyPresent)
                        {
                            this.<>2__current = this.<>4__this.nullKeyDictionary.nullKeyValue;
                            this.<>1__state = 3;
                            return true;
                        }
                    Label_00CE:
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
                                this.<>m__Finallyb();
                            }
                            return;
                    }
                }

                TypeValue IEnumerator<TypeValue>.Current
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
}

