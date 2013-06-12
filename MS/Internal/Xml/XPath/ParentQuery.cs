namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class ParentQuery : CacheAxisQuery
    {
        private ParentQuery(ParentQuery other) : base(other)
        {
        }

        public ParentQuery(Query qyInput, string Name, string Prefix, XPathNodeType Type) : base(qyInput, Name, Prefix, Type)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new ParentQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            XPathNavigator navigator;
            base.Evaluate(context);
            while ((navigator = base.qyInput.Advance()) != null)
            {
                navigator = navigator.Clone();
                if (navigator.MoveToParent() && this.matches(navigator))
                {
                    base.Insert(base.outputBuffer, navigator);
                }
            }
            return this;
        }
    }
}

