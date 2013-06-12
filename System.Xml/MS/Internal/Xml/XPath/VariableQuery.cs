namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class VariableQuery : ExtensionQuery
    {
        private IXsltContextVariable variable;

        private VariableQuery(VariableQuery other) : base(other)
        {
            this.variable = other.variable;
        }

        public VariableQuery(string name, string prefix) : base(prefix, name)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new VariableQuery(this);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            if (base.xsltContext == null)
            {
                throw XPathException.Create("Xp_NoContext");
            }
            return base.ProcessResult(this.variable.Evaluate(base.xsltContext));
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("name", (base.prefix.Length != 0) ? (base.prefix + ':' + base.name) : base.name);
            w.WriteEndElement();
        }

        public override void SetXsltContext(XsltContext context)
        {
            if (context == null)
            {
                throw XPathException.Create("Xp_NoContext");
            }
            if (base.xsltContext != context)
            {
                base.xsltContext = context;
                this.variable = base.xsltContext.ResolveVariable(base.prefix, base.name);
                if (this.variable == null)
                {
                    throw XPathException.Create("Xp_UndefVar", base.QName);
                }
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                if (this.variable != null)
                {
                    return base.GetXPathType(this.Evaluate(null));
                }
                XPathResultType any = (this.variable != null) ? this.variable.VariableType : XPathResultType.Any;
                if (any == XPathResultType.Error)
                {
                    any = XPathResultType.Any;
                }
                return any;
            }
        }
    }
}

