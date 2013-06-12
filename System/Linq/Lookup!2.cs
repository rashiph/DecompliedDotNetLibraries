namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class Lookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
    {
        private IEqualityComparer<TKey> comparer;
        private int count;
        private Grouping<TKey, TElement>[] groupings;
        private Grouping<TKey, TElement> lastGrouping;

        private Lookup(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }
            this.comparer = comparer;
            this.groupings = new Grouping<TKey, TElement>[7];
        }

        public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            Grouping<TKey, TElement> lastGrouping = this.lastGrouping;
            if (lastGrouping != null)
            {
                do
                {
                    lastGrouping = lastGrouping.next;
                    if (lastGrouping.count != lastGrouping.elements.Length)
                    {
                        Array.Resize<TElement>(ref lastGrouping.elements, lastGrouping.count);
                    }
                    yield return this.resultSelector(lastGrouping.key, lastGrouping.elements);
                }
                while (lastGrouping != this.lastGrouping);
            }
        }

        public bool Contains(TKey key)
        {
            return (this.GetGrouping(key, false) != null);
        }

        internal static Lookup<TKey, TElement> Create<TSource>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }
            if (keySelector == null)
            {
                throw Error.ArgumentNull("keySelector");
            }
            if (elementSelector == null)
            {
                throw Error.ArgumentNull("elementSelector");
            }
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            foreach (TSource local in source)
            {
                lookup.GetGrouping(keySelector(local), true).Add(elementSelector(local));
            }
            return lookup;
        }

        internal static Lookup<TKey, TElement> CreateForJoin(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            foreach (TElement local in source)
            {
                TKey key = keySelector(local);
                if (key != null)
                {
                    lookup.GetGrouping(key, true).Add(local);
                }
            }
            return lookup;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            Grouping<TKey, TElement> lastGrouping = this.lastGrouping;
            if (lastGrouping != null)
            {
                do
                {
                    lastGrouping = lastGrouping.next;
                    yield return lastGrouping;
                }
                while (lastGrouping != this.lastGrouping);
            }
        }

        internal Grouping<TKey, TElement> GetGrouping(TKey key, bool create)
        {
            int hashCode = this.InternalGetHashCode(key);
            for (Grouping<TKey, TElement> grouping = this.groupings[hashCode % this.groupings.Length]; grouping != null; grouping = grouping.hashNext)
            {
                if ((grouping.hashCode == hashCode) && this.comparer.Equals(grouping.key, key))
                {
                    return grouping;
                }
            }
            if (!create)
            {
                return null;
            }
            if (this.count == this.groupings.Length)
            {
                this.Resize();
            }
            int index = hashCode % this.groupings.Length;
            Grouping<TKey, TElement> grouping2 = new Grouping<TKey, TElement> {
                key = key,
                hashCode = hashCode,
                elements = new TElement[1],
                hashNext = this.groupings[index]
            };
            this.groupings[index] = grouping2;
            if (this.lastGrouping == null)
            {
                grouping2.next = grouping2;
            }
            else
            {
                grouping2.next = this.lastGrouping.next;
                this.lastGrouping.next = grouping2;
            }
            this.lastGrouping = grouping2;
            this.count++;
            return grouping2;
        }

        internal int InternalGetHashCode(TKey key)
        {
            if (key != null)
            {
                return (this.comparer.GetHashCode(key) & 0x7fffffff);
            }
            return 0;
        }

        private void Resize()
        {
            int num = (this.count * 2) + 1;
            Grouping<TKey, TElement>[] groupingArray = new Grouping<TKey, TElement>[num];
            Grouping<TKey, TElement> lastGrouping = this.lastGrouping;
            do
            {
                lastGrouping = lastGrouping.next;
                int index = lastGrouping.hashCode % num;
                lastGrouping.hashNext = groupingArray[index];
                groupingArray[index] = lastGrouping;
            }
            while (lastGrouping != this.lastGrouping);
            this.groupings = groupingArray;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                Grouping<TKey, TElement> grouping = this.GetGrouping(key, false);
                if (grouping != null)
                {
                    return grouping;
                }
                return EmptyEnumerable<TElement>.Instance;
            }
        }

        [CompilerGenerated]
        private sealed class <ApplyResultSelector>d__3<TResult> : IEnumerable<TResult>, IEnumerable, IEnumerator<TResult>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TResult <>2__current;
            public Func<TKey, IEnumerable<TElement>, TResult> <>3__resultSelector;
            public Lookup<TKey, TElement> <>4__this;
            private int <>l__initialThreadId;
            public Lookup<TKey, TElement>.Grouping <g>5__4;
            public Func<TKey, IEnumerable<TElement>, TResult> resultSelector;

            [DebuggerHidden]
            public <ApplyResultSelector>d__3(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<g>5__4 = this.<>4__this.lastGrouping;
                        if (this.<g>5__4 == null)
                        {
                            goto Label_00D0;
                        }
                        break;

                    case 1:
                        this.<>1__state = -1;
                        if (this.<g>5__4 != this.<>4__this.lastGrouping)
                        {
                            break;
                        }
                        goto Label_00D0;

                    default:
                        goto Label_00D0;
                }
                this.<g>5__4 = this.<g>5__4.next;
                if (this.<g>5__4.count != this.<g>5__4.elements.Length)
                {
                    Array.Resize<TElement>(ref this.<g>5__4.elements, this.<g>5__4.count);
                }
                this.<>2__current = this.resultSelector(this.<g>5__4.key, this.<g>5__4.elements);
                this.<>1__state = 1;
                return true;
            Label_00D0:
                return false;
            }

            [DebuggerHidden]
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                Lookup<TKey, TElement>.<ApplyResultSelector>d__3<TResult> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (Lookup<TKey, TElement>.<ApplyResultSelector>d__3<TResult>) this;
                }
                else
                {
                    d__ = new Lookup<TKey, TElement>.<ApplyResultSelector>d__3<TResult>(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.resultSelector = this.<>3__resultSelector;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TResult>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            TResult IEnumerator<TResult>.Current
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

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<IGrouping<TKey, TElement>>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private IGrouping<TKey, TElement> <>2__current;
            public Lookup<TKey, TElement> <>4__this;
            public Lookup<TKey, TElement>.Grouping <g>5__1;

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
                        this.<g>5__1 = this.<>4__this.lastGrouping;
                        if (this.<g>5__1 == null)
                        {
                            goto Label_0077;
                        }
                        break;

                    case 1:
                        this.<>1__state = -1;
                        if (this.<g>5__1 != this.<>4__this.lastGrouping)
                        {
                            break;
                        }
                        goto Label_0077;

                    default:
                        goto Label_0077;
                }
                this.<g>5__1 = this.<g>5__1.next;
                this.<>2__current = this.<g>5__1;
                this.<>1__state = 1;
                return true;
            Label_0077:
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

        internal class Grouping : IGrouping<TKey, TElement>, IList<TElement>, ICollection<TElement>, IEnumerable<TElement>, IEnumerable
        {
            internal int count;
            internal TElement[] elements;
            internal int hashCode;
            internal Lookup<TKey, TElement>.Grouping hashNext;
            internal TKey key;
            internal Lookup<TKey, TElement>.Grouping next;

            internal void Add(TElement element)
            {
                if (this.elements.Length == this.count)
                {
                    Array.Resize<TElement>(ref this.elements, this.count * 2);
                }
                this.elements[this.count] = element;
                this.count++;
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                int index = 0;
                while (true)
                {
                    if (index >= this.count)
                    {
                        yield break;
                    }
                    yield return this.elements[index];
                    index++;
                }
            }

            void ICollection<TElement>.Add(TElement item)
            {
                throw Error.NotSupported();
            }

            void ICollection<TElement>.Clear()
            {
                throw Error.NotSupported();
            }

            bool ICollection<TElement>.Contains(TElement item)
            {
                return (Array.IndexOf<TElement>(this.elements, item, 0, this.count) >= 0);
            }

            void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
            {
                Array.Copy(this.elements, 0, array, arrayIndex, this.count);
            }

            bool ICollection<TElement>.Remove(TElement item)
            {
                throw Error.NotSupported();
            }

            int IList<TElement>.IndexOf(TElement item)
            {
                return Array.IndexOf<TElement>(this.elements, item, 0, this.count);
            }

            void IList<TElement>.Insert(int index, TElement item)
            {
                throw Error.NotSupported();
            }

            void IList<TElement>.RemoveAt(int index)
            {
                throw Error.NotSupported();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public TKey Key
            {
                get
                {
                    return this.key;
                }
            }

            int ICollection<TElement>.Count
            {
                get
                {
                    return this.count;
                }
            }

            bool ICollection<TElement>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            TElement IList<TElement>.this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.count))
                    {
                        throw Error.ArgumentOutOfRange("index");
                    }
                    return this.elements[index];
                }
                set
                {
                    throw Error.NotSupported();
                }
            }

            [CompilerGenerated]
            private sealed class <GetEnumerator>d__7 : IEnumerator<TElement>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private TElement <>2__current;
                public Lookup<TKey, TElement>.Grouping <>4__this;
                public int <i>5__8;

                [DebuggerHidden]
                public <GetEnumerator>d__7(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                }

                private bool MoveNext()
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<i>5__8 = 0;
                            break;

                        case 1:
                            this.<>1__state = -1;
                            this.<i>5__8++;
                            break;

                        default:
                            goto Label_0074;
                    }
                    if (this.<i>5__8 < this.<>4__this.count)
                    {
                        this.<>2__current = this.<>4__this.elements[this.<i>5__8];
                        this.<>1__state = 1;
                        return true;
                    }
                Label_0074:
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

                TElement IEnumerator<TElement>.Current
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

