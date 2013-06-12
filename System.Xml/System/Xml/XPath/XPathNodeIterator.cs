namespace System.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    [DebuggerDisplay("Position={CurrentPosition}, Current={debuggerDisplayProxy}")]
    public abstract class XPathNodeIterator : ICloneable, IEnumerable
    {
        internal int count = -1;

        protected XPathNodeIterator()
        {
        }

        public abstract XPathNodeIterator Clone();
        public virtual IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public abstract bool MoveNext();
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual int Count
        {
            get
            {
                if (this.count == -1)
                {
                    XPathNodeIterator iterator = this.Clone();
                    while (iterator.MoveNext())
                    {
                    }
                    this.count = iterator.CurrentPosition;
                }
                return this.count;
            }
        }

        public abstract XPathNavigator Current { get; }

        public abstract int CurrentPosition { get; }

        private object debuggerDisplayProxy
        {
            get
            {
                if (this.Current != null)
                {
                    return new XPathNavigator.DebuggerDisplayProxy(this.Current);
                }
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DebuggerDisplayProxy
        {
            private XPathNodeIterator nodeIterator;
            public DebuggerDisplayProxy(XPathNodeIterator nodeIterator)
            {
                this.nodeIterator = nodeIterator;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Position=");
                builder.Append(this.nodeIterator.CurrentPosition);
                builder.Append(", Current=");
                if (this.nodeIterator.Current == null)
                {
                    builder.Append("null");
                }
                else
                {
                    builder.Append('{');
                    builder.Append(new XPathNavigator.DebuggerDisplayProxy(this.nodeIterator.Current).ToString());
                    builder.Append('}');
                }
                return builder.ToString();
            }
        }

        private class Enumerator : IEnumerator
        {
            private XPathNodeIterator current;
            private bool iterationStarted;
            private XPathNodeIterator original;

            public Enumerator(XPathNodeIterator original)
            {
                this.original = original.Clone();
            }

            public virtual bool MoveNext()
            {
                if (!this.iterationStarted)
                {
                    this.current = this.original.Clone();
                    this.iterationStarted = true;
                }
                if ((this.current != null) && this.current.MoveNext())
                {
                    return true;
                }
                this.current = null;
                return false;
            }

            public virtual void Reset()
            {
                this.iterationStarted = false;
            }

            public virtual object Current
            {
                get
                {
                    if (!this.iterationStarted)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumNotStarted", new object[] { string.Empty }));
                    }
                    if (this.current == null)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumFinished", new object[] { string.Empty }));
                    }
                    return this.current.Current.Clone();
                }
            }
        }
    }
}

