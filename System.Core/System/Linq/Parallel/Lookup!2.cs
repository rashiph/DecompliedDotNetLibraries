namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        private IEqualityComparer<TKey> m_comparer;
        private IGrouping<TKey, TElement> m_defaultKeyGrouping;
        private IDictionary<TKey, IGrouping<TKey, TElement>> m_dict;

        internal Lookup(IEqualityComparer<TKey> comparer)
        {
            this.m_comparer = comparer;
            this.m_dict = new Dictionary<TKey, IGrouping<TKey, TElement>>(this.m_comparer);
        }

        internal void Add(IGrouping<TKey, TElement> grouping)
        {
            if (this.m_comparer.Equals(grouping.Key, default(TKey)))
            {
                this.m_defaultKeyGrouping = grouping;
            }
            else
            {
                this.m_dict.Add(grouping.Key, grouping);
            }
        }

        public bool Contains(TKey key)
        {
            if (this.m_comparer.Equals(key, default(TKey)))
            {
                return (this.m_defaultKeyGrouping != null);
            }
            return this.m_dict.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            foreach (IGrouping<TKey, TElement> iteratorVariable0 in this.m_dict.Values)
            {
                yield return iteratorVariable0;
            }
            if (this.m_defaultKeyGrouping != null)
            {
                yield return this.m_defaultKeyGrouping;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                int count = this.m_dict.Count;
                if (this.m_defaultKeyGrouping != null)
                {
                    count++;
                }
                return count;
            }
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                IGrouping<TKey, TElement> grouping;
                if (this.m_comparer.Equals(key, default(TKey)))
                {
                    if (this.m_defaultKeyGrouping != null)
                    {
                        return this.m_defaultKeyGrouping;
                    }
                    return Enumerable.Empty<TElement>();
                }
                if (this.m_dict.TryGetValue(key, out grouping))
                {
                    return grouping;
                }
                return Enumerable.Empty<TElement>();
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<IGrouping<TKey, TElement>>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private IGrouping<TKey, TElement> <>2__current;
            public System.Linq.Parallel.Lookup<TKey, TElement> <>4__this;
            public IEnumerator<IGrouping<TKey, TElement>> <>7__wrap2;
            public IGrouping<TKey, TElement> <grouping>5__1;

            [DebuggerHidden]
            public <GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private void <>m__Finally3()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap2 != null)
                {
                    this.<>7__wrap2.Dispose();
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
                            this.<>7__wrap2 = this.<>4__this.m_dict.Values.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_007C;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_007C;

                        case 3:
                            this.<>1__state = -1;
                            goto Label_00BF;

                        default:
                            goto Label_00BF;
                    }
                Label_004D:
                    this.<grouping>5__1 = this.<>7__wrap2.Current;
                    this.<>2__current = this.<grouping>5__1;
                    this.<>1__state = 2;
                    return true;
                Label_007C:
                    if (this.<>7__wrap2.MoveNext())
                    {
                        goto Label_004D;
                    }
                    this.<>m__Finally3();
                    if (this.<>4__this.m_defaultKeyGrouping != null)
                    {
                        this.<>2__current = this.<>4__this.m_defaultKeyGrouping;
                        this.<>1__state = 3;
                        return true;
                    }
                Label_00BF:
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
                            this.<>m__Finally3();
                        }
                        return;
                }
            }

            IGrouping<TKey, TElement> IEnumerator<IGrouping<TKey, TElement>>.Current
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

