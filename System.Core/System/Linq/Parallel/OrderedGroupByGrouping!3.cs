namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class OrderedGroupByGrouping<TGroupKey, TOrderKey, TElement> : IGrouping<TGroupKey, TElement>, IEnumerable<TElement>, IEnumerable
    {
        private TGroupKey m_groupKey;
        private IComparer<TOrderKey> m_orderComparer;
        private GrowingArray<TOrderKey> m_orderKeys;
        private GrowingArray<TElement> m_values;

        internal OrderedGroupByGrouping(TGroupKey groupKey, IComparer<TOrderKey> orderComparer)
        {
            this.m_groupKey = groupKey;
            this.m_values = new GrowingArray<TElement>();
            this.m_orderKeys = new GrowingArray<TOrderKey>();
            this.m_orderComparer = orderComparer;
        }

        internal void Add(TElement value, TOrderKey orderKey)
        {
            this.m_values.Add(value);
            this.m_orderKeys.Add(orderKey);
        }

        internal void DoneAdding()
        {
            Array.Sort<TOrderKey, TElement>(this.m_orderKeys.InternalArray, this.m_values.InternalArray, 0, this.m_values.Count, this.m_orderComparer);
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            int count = this.m_values.Count;
            TElement[] internalArray = this.m_values.InternalArray;
            int index = 0;
            while (true)
            {
                if (index >= count)
                {
                    yield break;
                }
                yield return internalArray[index];
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TElement>) this).GetEnumerator();
        }

        TGroupKey IGrouping<TGroupKey, TElement>.Key
        {
            get
            {
                return this.m_groupKey;
            }
        }

        [CompilerGenerated]
        private sealed class GetEnumerator>d__0 : IEnumerator<TElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TElement <>2__current;
            public OrderedGroupByGrouping<TGroupKey, TOrderKey, TElement> <>4__this;
            public int <i>5__3;
            public TElement[] <valueArray>5__2;
            public int <valueCount>5__1;

            [DebuggerHidden]
            public GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<valueCount>5__1 = this.<>4__this.m_values.Count;
                        this.<valueArray>5__2 = this.<>4__this.m_values.InternalArray;
                        this.<i>5__3 = 0;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<i>5__3++;
                        break;

                    default:
                        goto Label_0096;
                }
                if (this.<i>5__3 < this.<valueCount>5__1)
                {
                    this.<>2__current = this.<valueArray>5__2[this.<i>5__3];
                    this.<>1__state = 1;
                    return true;
                }
            Label_0096:
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

