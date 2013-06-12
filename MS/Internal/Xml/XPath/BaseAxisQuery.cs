namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal abstract class BaseAxisQuery : Query
    {
        protected XPathNavigator currentNode;
        private string name;
        private bool nameTest;
        private string nsUri;
        protected int position;
        private string prefix;
        internal Query qyInput;
        private XPathNodeType typeTest;

        protected BaseAxisQuery(BaseAxisQuery other) : base(other)
        {
            this.qyInput = Query.Clone(other.qyInput);
            this.name = other.name;
            this.prefix = other.prefix;
            this.nsUri = other.nsUri;
            this.typeTest = other.typeTest;
            this.nameTest = other.nameTest;
            this.position = other.position;
            this.currentNode = other.currentNode;
        }

        protected BaseAxisQuery(Query qyInput)
        {
            this.name = string.Empty;
            this.prefix = string.Empty;
            this.nsUri = string.Empty;
            this.qyInput = qyInput;
        }

        protected BaseAxisQuery(Query qyInput, string name, string prefix, XPathNodeType typeTest)
        {
            this.qyInput = qyInput;
            this.name = name;
            this.prefix = prefix;
            this.typeTest = typeTest;
            this.nameTest = (prefix.Length != 0) || (name.Length != 0);
            this.nsUri = string.Empty;
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            base.ResetCount();
            this.Reset();
            this.qyInput.Evaluate(nodeIterator);
            return this;
        }

        public virtual bool matches(XPathNavigator e)
        {
            if (((this.TypeTest == e.NodeType) || (this.TypeTest == XPathNodeType.All)) || ((this.TypeTest == XPathNodeType.Text) && ((e.NodeType == XPathNodeType.Whitespace) || (e.NodeType == XPathNodeType.SignificantWhitespace))))
            {
                if (!this.NameTest)
                {
                    return true;
                }
                if ((this.name.Equals(e.LocalName) || (this.name.Length == 0)) && this.nsUri.Equals(e.NamespaceURI))
                {
                    return true;
                }
            }
            return false;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            if (this.NameTest)
            {
                w.WriteAttributeString("name", (this.Prefix.Length != 0) ? (this.Prefix + ':' + this.Name) : this.Name);
            }
            if (this.TypeTest != XPathNodeType.Element)
            {
                w.WriteAttributeString("nodeType", this.TypeTest.ToString());
            }
            this.qyInput.PrintQuery(w);
            w.WriteEndElement();
        }

        public override void Reset()
        {
            this.position = 0;
            this.currentNode = null;
            this.qyInput.Reset();
        }

        public override void SetXsltContext(XsltContext context)
        {
            this.nsUri = context.LookupNamespace(this.prefix);
            this.qyInput.SetXsltContext(context);
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.currentNode;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.position;
            }
        }

        protected string Name
        {
            get
            {
                return this.name;
            }
        }

        protected string Namespace
        {
            get
            {
                return this.nsUri;
            }
        }

        protected bool NameTest
        {
            get
            {
                return this.nameTest;
            }
        }

        protected string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }

        protected XPathNodeType TypeTest
        {
            get
            {
                return this.typeTest;
            }
        }

        public override double XsltDefaultPriority
        {
            get
            {
                if (this.qyInput.GetType() != typeof(ContextQuery))
                {
                    return 0.5;
                }
                if (this.name.Length != 0)
                {
                    return 0.0;
                }
                if (this.prefix.Length != 0)
                {
                    return -0.25;
                }
                return -0.5;
            }
        }
    }
}

