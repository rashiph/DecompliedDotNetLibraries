namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class XPathAncestorQuery : CacheAxisQuery
    {
        private bool matchSelf;

        private XPathAncestorQuery(XPathAncestorQuery other) : base(other)
        {
            this.matchSelf = other.matchSelf;
        }

        public XPathAncestorQuery(Query qyInput, string name, string prefix, XPathNodeType typeTest, bool matchSelf) : base(qyInput, name, prefix, typeTest)
        {
            this.matchSelf = matchSelf;
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathAncestorQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            XPathNavigator navigator2;
            base.Evaluate(context);
            XPathNavigator e = null;
            while ((navigator2 = base.qyInput.Advance()) != null)
            {
                if ((!this.matchSelf || !this.matches(navigator2)) || base.Insert(base.outputBuffer, navigator2))
                {
                    if ((e == null) || !e.MoveTo(navigator2))
                    {
                        e = navigator2.Clone();
                    }
                    while (e.MoveToParent())
                    {
                        if (this.matches(e) && !base.Insert(base.outputBuffer, e))
                        {
                            continue;
                        }
                    }
                }
            }
            return this;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            if (this.matchSelf)
            {
                w.WriteAttributeString("self", "yes");
            }
            if (base.NameTest)
            {
                w.WriteAttributeString("name", (base.Prefix.Length != 0) ? (base.Prefix + ':' + base.Name) : base.Name);
            }
            if (base.TypeTest != XPathNodeType.Element)
            {
                w.WriteAttributeString("nodeType", base.TypeTest.ToString());
            }
            base.qyInput.PrintQuery(w);
            w.WriteEndElement();
        }

        public override int CurrentPosition
        {
            get
            {
                return ((base.outputBuffer.Count - base.count) + 1);
            }
        }

        public override QueryProps Properties
        {
            get
            {
                return (base.Properties | QueryProps.Reverse);
            }
        }
    }
}

