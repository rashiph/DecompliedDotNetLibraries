namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;

    [DebuggerDisplay("Position={CurrentPosition}, Current={debuggerDisplayProxy, nq}")]
    internal class XPathArrayIterator : ResetableIterator
    {
        protected int index;
        protected IList list;

        public XPathArrayIterator(XPathArrayIterator it)
        {
            this.list = it.list;
            this.index = it.index;
        }

        public XPathArrayIterator(IList list)
        {
            this.list = list;
        }

        public XPathArrayIterator(XPathNodeIterator nodeIterator)
        {
            this.list = new ArrayList();
            while (nodeIterator.MoveNext())
            {
                this.list.Add(nodeIterator.Current.Clone());
            }
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathArrayIterator(this);
        }

        public override IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public override bool MoveNext()
        {
            if (this.index == this.list.Count)
            {
                return false;
            }
            this.index++;
            return true;
        }

        public override void Reset()
        {
            this.index = 0;
        }

        public IList AsList
        {
            get
            {
                return this.list;
            }
        }

        public override int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                if (this.index < 1)
                {
                    throw new InvalidOperationException(Res.GetString("Sch_EnumNotStarted", new object[] { string.Empty }));
                }
                return (XPathNavigator) this.list[this.index - 1];
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.index;
            }
        }

        private object debuggerDisplayProxy
        {
            get
            {
                if (this.index >= 1)
                {
                    return new XPathNavigator.DebuggerDisplayProxy(this.Current);
                }
                return null;
            }
        }
    }
}

