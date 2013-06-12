namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class FunctionQuery : ExtensionQuery
    {
        private IList<Query> args;
        private IXsltContextFunction function;

        private FunctionQuery(FunctionQuery other) : base(other)
        {
            this.function = other.function;
            Query[] queryArray = new Query[other.args.Count];
            for (int i = 0; i < queryArray.Length; i++)
            {
                queryArray[i] = Query.Clone(other.args[i]);
            }
            this.args = queryArray;
            this.args = queryArray;
        }

        public FunctionQuery(string prefix, string name, List<Query> args) : base(prefix, name)
        {
            this.args = args;
        }

        public override XPathNodeIterator Clone()
        {
            return new FunctionQuery(this);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            object obj2;
            if (base.xsltContext == null)
            {
                throw XPathException.Create("Xp_NoContext");
            }
            object[] args = new object[this.args.Count];
            for (int i = 0; i < this.args.Count; i++)
            {
                args[i] = this.args[i].Evaluate(nodeIterator);
                if (args[i] is XPathNodeIterator)
                {
                    args[i] = new XPathSelectionIterator(nodeIterator.Current, this.args[i]);
                }
            }
            try
            {
                obj2 = base.ProcessResult(this.function.Invoke(base.xsltContext, args, nodeIterator.Current));
            }
            catch (Exception exception)
            {
                throw XPathException.Create("Xp_FunctionFailed", base.QName, exception);
            }
            return obj2;
        }

        public override XPathNavigator MatchNode(XPathNavigator navigator)
        {
            if ((base.name != "key") && (base.prefix.Length != 0))
            {
                throw XPathException.Create("Xp_InvalidPattern");
            }
            this.Evaluate(new XPathSingletonIterator(navigator, true));
            XPathNavigator navigator2 = null;
            while ((navigator2 = this.Advance()) != null)
            {
                if (navigator2.IsSamePosition(navigator))
                {
                    return navigator2;
                }
            }
            return navigator2;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("name", (base.prefix.Length != 0) ? (base.prefix + ':' + base.name) : base.name);
            foreach (Query query in this.args)
            {
                query.PrintQuery(w);
            }
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
                foreach (Query query in this.args)
                {
                    query.SetXsltContext(context);
                }
                XPathResultType[] argTypes = new XPathResultType[this.args.Count];
                for (int i = 0; i < this.args.Count; i++)
                {
                    argTypes[i] = this.args[i].StaticType;
                }
                this.function = base.xsltContext.ResolveFunction(base.prefix, base.name, argTypes);
                if (this.function == null)
                {
                    throw XPathException.Create("Xp_UndefFunc", base.QName);
                }
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                XPathResultType any = (this.function != null) ? this.function.ReturnType : XPathResultType.Any;
                if (any == XPathResultType.Error)
                {
                    any = XPathResultType.Any;
                }
                return any;
            }
        }
    }
}

