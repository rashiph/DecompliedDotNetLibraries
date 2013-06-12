namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class NamespaceQuery : BaseAxisQuery
    {
        private bool onNamespace;

        private NamespaceQuery(NamespaceQuery other) : base((BaseAxisQuery) other)
        {
            this.onNamespace = other.onNamespace;
        }

        public NamespaceQuery(Query qyParent, string Name, string Prefix, XPathNodeType Type) : base(qyParent, Name, Prefix, Type)
        {
        }

        public override XPathNavigator Advance()
        {
            do
            {
                if (!this.onNamespace)
                {
                    base.currentNode = base.qyInput.Advance();
                    if (base.currentNode == null)
                    {
                        return null;
                    }
                    base.position = 0;
                    base.currentNode = base.currentNode.Clone();
                    this.onNamespace = base.currentNode.MoveToFirstNamespace();
                }
                else
                {
                    this.onNamespace = base.currentNode.MoveToNextNamespace();
                }
            }
            while (!this.onNamespace || !this.matches(base.currentNode));
            base.position++;
            return base.currentNode;
        }

        public override XPathNodeIterator Clone()
        {
            return new NamespaceQuery(this);
        }

        public override bool matches(XPathNavigator e)
        {
            if (e.Value.Length == 0)
            {
                return false;
            }
            if (base.NameTest)
            {
                return base.Name.Equals(e.LocalName);
            }
            return true;
        }

        public override void Reset()
        {
            this.onNamespace = false;
            base.Reset();
        }
    }
}

