namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class ExceptionAggregator
    {
        private static bool ThrowAnOCE(Exception ex, CancellationState cancellationState)
        {
            OperationCanceledException exception = ex as OperationCanceledException;
            return ((((exception != null) && (exception.CancellationToken == cancellationState.ExternalCancellationToken)) && cancellationState.ExternalCancellationToken.IsCancellationRequested) || (((exception != null) && (exception.CancellationToken == cancellationState.MergedCancellationToken)) && (cancellationState.MergedCancellationToken.IsCancellationRequested && cancellationState.ExternalCancellationToken.IsCancellationRequested)));
        }

        internal static void ThrowOCEorAggregateException(Exception ex, CancellationState cancellationState)
        {
            if (!ThrowAnOCE(ex, cancellationState))
            {
                throw new AggregateException(new Exception[] { ex });
            }
            CancellationState.ThrowWithStandardMessageIfCanceled(cancellationState.ExternalCancellationToken);
        }

        internal static IEnumerable<TElement> WrapEnumerable<TElement>(IEnumerable<TElement> source, CancellationState cancellationState)
        {
            IEnumerator<TElement> enumerator = source.GetEnumerator();
            while (true)
            {
                TElement current = default(TElement);
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    ThrowOCEorAggregateException(exception, cancellationState);
                }
                yield return current;
            }
        }

        internal static Func<T, U> WrapFunc<T, U>(Func<T, U> f, CancellationState cancellationState)
        {
            return delegate (T t) {
                U local = default(U);
                try
                {
                    local = f(t);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    ThrowOCEorAggregateException(exception, cancellationState);
                }
                return local;
            };
        }

        internal static IEnumerable<TElement> WrapQueryEnumerator<TElement, TIgnoreKey>(QueryOperatorEnumerator<TElement, TIgnoreKey> source, CancellationState cancellationState)
        {
            TElement currentElement = default(TElement);
            TIgnoreKey currentKey = default(TIgnoreKey);
            while (true)
            {
                try
                {
                    if (!source.MoveNext(ref currentElement, ref currentKey))
                    {
                        break;
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    ThrowOCEorAggregateException(exception, cancellationState);
                }
                yield return currentElement;
            }
        }

        [CompilerGenerated]
        private sealed class <WrapEnumerable>d__0<TElement> : IEnumerable<TElement>, IEnumerable, IEnumerator<TElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TElement <>2__current;
            public CancellationState <>3__cancellationState;
            public IEnumerable<TElement> <>3__source;
            private int <>l__initialThreadId;
            public TElement <elem>5__2;
            public IEnumerator<TElement> <enumerator>5__1;
            public CancellationState cancellationState;
            public IEnumerable<TElement> source;

            [DebuggerHidden]
            public <WrapEnumerable>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally3()
            {
                this.<>1__state = -1;
                if (this.<enumerator>5__1 != null)
                {
                    this.<enumerator>5__1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num != 0)
                    {
                        if (num == 3)
                        {
                            goto Label_0093;
                        }
                        goto Label_009C;
                    }
                    this.<>1__state = -1;
                    this.<enumerator>5__1 = this.source.GetEnumerator();
                    this.<>1__state = 1;
                Label_0036:
                    this.<elem>5__2 = default(TElement);
                    try
                    {
                        if (!this.<enumerator>5__1.MoveNext())
                        {
                            this.System.IDisposable.Dispose();
                            goto Label_009C;
                        }
                        this.<elem>5__2 = this.<enumerator>5__1.Current;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        ExceptionAggregator.ThrowOCEorAggregateException(exception, this.cancellationState);
                    }
                    this.<>2__current = this.<elem>5__2;
                    this.<>1__state = 3;
                    return true;
                Label_0093:
                    this.<>1__state = 1;
                    goto Label_0036;
                Label_009C:
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
                ExceptionAggregator.<WrapEnumerable>d__0<TElement> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (ExceptionAggregator.<WrapEnumerable>d__0<TElement>) this;
                }
                else
                {
                    d__ = new ExceptionAggregator.<WrapEnumerable>d__0<TElement>(0);
                }
                d__.source = this.<>3__source;
                d__.cancellationState = this.<>3__cancellationState;
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
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally3();
                        }
                        break;

                    case 2:
                        break;

                    default:
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

        [CompilerGenerated]
        private sealed class <WrapQueryEnumerator>d__6<TElement, TIgnoreKey> : IEnumerable<TElement>, IEnumerable, IEnumerator<TElement>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private TElement <>2__current;
            public CancellationState <>3__cancellationState;
            public QueryOperatorEnumerator<TElement, TIgnoreKey> <>3__source;
            private int <>l__initialThreadId;
            public TElement <elem>5__7;
            public TIgnoreKey <ignoreKey>5__8;
            public CancellationState cancellationState;
            public QueryOperatorEnumerator<TElement, TIgnoreKey> source;

            [DebuggerHidden]
            public <WrapQueryEnumerator>d__6(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally9()
            {
                this.<>1__state = -1;
                this.source.Dispose();
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num != 0)
                    {
                        if (num == 3)
                        {
                            goto Label_0083;
                        }
                        goto Label_008C;
                    }
                    this.<>1__state = -1;
                    this.<elem>5__7 = default(TElement);
                    this.<ignoreKey>5__8 = default(TIgnoreKey);
                    this.<>1__state = 1;
                Label_0037:
                    try
                    {
                        if (!this.source.MoveNext(ref this.<elem>5__7, ref this.<ignoreKey>5__8))
                        {
                            this.System.IDisposable.Dispose();
                            goto Label_008C;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        ExceptionAggregator.ThrowOCEorAggregateException(exception, this.cancellationState);
                    }
                    this.<>2__current = this.<elem>5__7;
                    this.<>1__state = 3;
                    return true;
                Label_0083:
                    this.<>1__state = 1;
                    goto Label_0037;
                Label_008C:
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
                ExceptionAggregator.<WrapQueryEnumerator>d__6<TElement, TIgnoreKey> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (ExceptionAggregator.<WrapQueryEnumerator>d__6<TElement, TIgnoreKey>) this;
                }
                else
                {
                    d__ = new ExceptionAggregator.<WrapQueryEnumerator>d__6<TElement, TIgnoreKey>(0);
                }
                d__.source = this.<>3__source;
                d__.cancellationState = this.<>3__cancellationState;
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
                    case 3:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally9();
                        }
                        break;

                    case 2:
                        break;

                    default:
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

