namespace System.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct XPathEvaluator
    {
        public object Evaluate<T>(XNode node, string expression, IXmlNamespaceResolver resolver) where T: class
        {
            object obj2 = node.CreateNavigator().Evaluate(expression, resolver);
            if (obj2 is XPathNodeIterator)
            {
                return this.EvaluateIterator<T>((XPathNodeIterator) obj2);
            }
            if (!(obj2 is T))
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnexpectedEvaluation", new object[] { obj2.GetType() }));
            }
            return (T) obj2;
        }

        private IEnumerable<T> EvaluateIterator<T>(XPathNodeIterator result)
        {
            IEnumerator enumerator = result.GetEnumerator();
            while (enumerator.MoveNext())
            {
                XPathNavigator current = (XPathNavigator) enumerator.Current;
                object underlyingObject = current.UnderlyingObject;
                if (!(underlyingObject is T))
                {
                    throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnexpectedEvaluation", new object[] { underlyingObject.GetType() }));
                }
                yield return (T) underlyingObject;
                XText next = underlyingObject as XText;
                if ((next != null) && (next.parent != null))
                {
                    while (next != next.parent.content)
                    {
                        next = next.next as XText;
                        if (next != null)
                        {
                            yield return (T) next;
                        }
                    }
                }
            }
        }
        [CompilerGenerated]
        private sealed class <EvaluateIterator>d__0<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public XPathNodeIterator <>3__result;
            public XPathEvaluator <>4__this;
            public IEnumerator <>7__wrap4;
            public IDisposable <>7__wrap5;
            private int <>l__initialThreadId;
            public XPathNavigator <navigator>5__1;
            public object <r>5__2;
            public XText <t>5__3;
            public XPathNodeIterator result;

            [DebuggerHidden]
            public <EvaluateIterator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally6()
            {
                this.<>1__state = -1;
                this.<>7__wrap5 = this.<>7__wrap4 as IDisposable;
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
                            this.<>7__wrap4 = this.result.GetEnumerator();
                            this.<>1__state = 1;
                            goto Label_0147;

                        case 2:
                            this.<>1__state = 1;
                            this.<t>5__3 = this.<r>5__2 as XText;
                            if ((this.<t>5__3 == null) || (this.<t>5__3.parent == null))
                            {
                                goto Label_0147;
                            }
                            goto Label_012F;

                        case 3:
                            this.<>1__state = 1;
                            goto Label_012F;

                        default:
                            goto Label_015D;
                    }
                Label_0046:
                    this.<navigator>5__1 = (XPathNavigator) this.<>7__wrap4.Current;
                    this.<r>5__2 = this.<navigator>5__1.UnderlyingObject;
                    if (!(this.<r>5__2 is T))
                    {
                        throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_UnexpectedEvaluation", new object[] { this.<r>5__2.GetType() }));
                    }
                    this.<>2__current = (T) this.<r>5__2;
                    this.<>1__state = 2;
                    return true;
                Label_00EE:
                    this.<t>5__3 = this.<t>5__3.next as XText;
                    if (this.<t>5__3 == null)
                    {
                        goto Label_0147;
                    }
                    this.<>2__current = (T) this.<t>5__3;
                    this.<>1__state = 3;
                    return true;
                Label_012F:
                    if (this.<t>5__3 != this.<t>5__3.parent.content)
                    {
                        goto Label_00EE;
                    }
                Label_0147:
                    if (this.<>7__wrap4.MoveNext())
                    {
                        goto Label_0046;
                    }
                    this.<>m__Finally6();
                Label_015D:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                XPathEvaluator.<EvaluateIterator>d__0<T> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (XPathEvaluator.<EvaluateIterator>d__0<T>) this;
                }
                else
                {
                    d__ = new XPathEvaluator.<EvaluateIterator>d__0<T>(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.result = this.<>3__result;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
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
                    case 3:
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

            T IEnumerator<T>.Current
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

