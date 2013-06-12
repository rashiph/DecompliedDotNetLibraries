namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>, IEnumerable<TElement>, IEnumerable
    {
        internal IEnumerable<TElement> source;

        protected OrderedEnumerable()
        {
        }

        internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next);
        public IEnumerator<TElement> GetEnumerator()
        {
            Buffer<TElement> iteratorVariable0 = new Buffer<TElement>(this.source);
            if (iteratorVariable0.count <= 0)
            {
                goto Label_00EA;
            }
            int[] iteratorVariable2 = this.GetEnumerableSorter(null).Sort(iteratorVariable0.items, iteratorVariable0.count);
            int index = 0;
        Label_PostSwitchInIterator:;
            if (index < iteratorVariable0.count)
            {
                yield return iteratorVariable0.items[iteratorVariable2[index]];
                index++;
                goto Label_PostSwitchInIterator;
            }
        Label_00EA:;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return new OrderedEnumerable<TElement, TKey>(this.source, keySelector, comparer, descending) { parent = (OrderedEnumerable<TElement>) this };
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<TElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TElement <>2__current;
            public OrderedEnumerable<TElement> <>4__this;
            public Buffer<TElement> <buffer>5__1;
            public int <i>5__4;
            public int[] <map>5__3;
            public EnumerableSorter<TElement> <sorter>5__2;

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
                        this.<buffer>5__1 = new Buffer<TElement>(this.<>4__this.source);
                        if (this.<buffer>5__1.count <= 0)
                        {
                            goto Label_00EA;
                        }
                        this.<sorter>5__2 = this.<>4__this.GetEnumerableSorter(null);
                        this.<map>5__3 = this.<sorter>5__2.Sort(this.<buffer>5__1.items, this.<buffer>5__1.count);
                        this.<sorter>5__2 = null;
                        this.<i>5__4 = 0;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<i>5__4++;
                        break;

                    default:
                        goto Label_00EA;
                }
                if (this.<i>5__4 < this.<buffer>5__1.count)
                {
                    this.<>2__current = this.<buffer>5__1.items[this.<map>5__3[this.<i>5__4]];
                    this.<>1__state = 1;
                    return true;
                }
            Label_00EA:
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

