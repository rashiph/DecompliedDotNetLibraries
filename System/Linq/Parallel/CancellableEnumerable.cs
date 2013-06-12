namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class CancellableEnumerable
    {
        internal static IEnumerable<TElement> Wrap<TElement>(IEnumerable<TElement> source, CancellationToken token)
        {
            int iteratorVariable0 = 0;
            foreach (TElement iteratorVariable1 in source)
            {
                if ((iteratorVariable0++ & 0x3f) == 0)
                {
                    CancellationState.ThrowIfCanceled(token);
                }
                yield return iteratorVariable1;
            }
        }

        [CompilerGenerated]
        private sealed class <Wrap>d__0<TElement> : IEnumerable<TElement>, IEnumerable, IEnumerator<TElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TElement <>2__current;
            public IEnumerable<TElement> <>3__source;
            public CancellationToken <>3__token;
            public IEnumerator<TElement> <>7__wrap3;
            private int <>l__initialThreadId;
            public int <count>5__1;
            public TElement <element>5__2;
            public IEnumerable<TElement> source;
            public CancellationToken token;

            [DebuggerHidden]
            public <Wrap>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally4()
            {
                this.<>1__state = -1;
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
                            this.<count>5__1 = 0;
                            this.<>7__wrap3 = this.source.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0096;

                        case 2:
                            this.<>1__state = 1;
                            goto Label_0096;

                        default:
                            goto Label_00A9;
                    }
                Label_0046:
                    this.<element>5__2 = this.<>7__wrap3.Current;
                    if ((this.<count>5__1++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.token);
                    }
                    this.<>2__current = this.<element>5__2;
                    this.<>1__state = 2;
                    return true;
                Label_0096:
                    if (this.<>7__wrap3.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finally4();
                Label_00A9:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
            {
                CancellableEnumerable.<Wrap>d__0<TElement> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (CancellableEnumerable.<Wrap>d__0<TElement>) this;
                }
                else
                {
                    d__ = new CancellableEnumerable.<Wrap>d__0<TElement>(0);
                }
                d__.source = this.<>3__source;
                d__.token = this.<>3__token;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TElement>.GetEnumerator();
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

