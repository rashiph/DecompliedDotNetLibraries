namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class ListChunk<TInputOutput> : IEnumerable<TInputOutput>, IEnumerable
    {
        internal TInputOutput[] m_chunk;
        private int m_chunkCount;
        private ListChunk<TInputOutput> m_nextChunk;
        private ListChunk<TInputOutput> m_tailChunk;

        internal ListChunk(int size)
        {
            this.m_chunk = new TInputOutput[size];
            this.m_chunkCount = 0;
            this.m_tailChunk = (ListChunk<TInputOutput>) this;
        }

        internal void Add(TInputOutput e)
        {
            ListChunk<TInputOutput> tailChunk = this.m_tailChunk;
            if (tailChunk.m_chunkCount == tailChunk.m_chunk.Length)
            {
                this.m_tailChunk = new ListChunk<TInputOutput>(tailChunk.m_chunkCount * 2);
                tailChunk = tailChunk.m_nextChunk = this.m_tailChunk;
            }
            tailChunk.m_chunk[tailChunk.m_chunkCount++] = e;
        }

        public IEnumerator<TInputOutput> GetEnumerator()
        {
            for (ListChunk<TInputOutput> iteratorVariable0 = (ListChunk<TInputOutput>) this; iteratorVariable0 != null; iteratorVariable0 = iteratorVariable0.m_nextChunk)
            {
                for (int i = 0; i < iteratorVariable0.m_chunkCount; i++)
                {
                    yield return iteratorVariable0.m_chunk[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal int Count
        {
            get
            {
                return this.m_chunkCount;
            }
        }

        internal ListChunk<TInputOutput> Next
        {
            get
            {
                return this.m_nextChunk;
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<TInputOutput>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TInputOutput <>2__current;
            public ListChunk<TInputOutput> <>4__this;
            public ListChunk<TInputOutput> <curr>5__1;
            public int <i>5__2;

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
                        this.<curr>5__1 = this.<>4__this;
                        while (this.<curr>5__1 != null)
                        {
                            this.<i>5__2 = 0;
                            while (this.<i>5__2 < this.<curr>5__1.m_chunkCount)
                            {
                                this.<>2__current = this.<curr>5__1.m_chunk[this.<i>5__2];
                                this.<>1__state = 1;
                                return true;
                            Label_005D:
                                this.<>1__state = -1;
                                this.<i>5__2++;
                            }
                            this.<curr>5__1 = this.<curr>5__1.m_nextChunk;
                        }
                        break;

                    case 1:
                        goto Label_005D;
                }
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

            TInputOutput IEnumerator<TInputOutput>.Current
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

