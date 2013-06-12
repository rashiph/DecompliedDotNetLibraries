namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class NodeFunctions : ValueQuery
    {
        private Query arg;
        private Function.FunctionType funcType;
        private XsltContext xsltContext;

        public NodeFunctions(Function.FunctionType funcType, Query arg)
        {
            this.funcType = funcType;
            this.arg = arg;
        }

        public override XPathNodeIterator Clone()
        {
            return new NodeFunctions(this.funcType, Query.Clone(this.arg)) { xsltContext = this.xsltContext };
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            XPathNavigator navigator;
            int num;
            switch (this.funcType)
            {
                case Function.FunctionType.FuncLast:
                    return (double) context.Count;

                case Function.FunctionType.FuncPosition:
                    return (double) context.CurrentPosition;

                case Function.FunctionType.FuncCount:
                    XPathNavigator navigator2;
                    this.arg.Evaluate(context);
                    num = 0;
                    if (this.xsltContext == null)
                    {
                        while (this.arg.Advance() != null)
                        {
                            num++;
                        }
                        break;
                    }
                    while ((navigator2 = this.arg.Advance()) != null)
                    {
                        if ((navigator2.NodeType != XPathNodeType.Whitespace) || this.xsltContext.PreserveWhitespace(navigator2))
                        {
                            num++;
                        }
                    }
                    break;

                case Function.FunctionType.FuncLocalName:
                    navigator = this.EvaluateArg(context);
                    if (navigator == null)
                    {
                        goto Label_00DF;
                    }
                    return navigator.LocalName;

                case Function.FunctionType.FuncNameSpaceUri:
                    navigator = this.EvaluateArg(context);
                    if (navigator == null)
                    {
                        goto Label_00DF;
                    }
                    return navigator.NamespaceURI;

                case Function.FunctionType.FuncName:
                    navigator = this.EvaluateArg(context);
                    if (navigator == null)
                    {
                        goto Label_00DF;
                    }
                    return navigator.Name;

                default:
                    goto Label_00DF;
            }
            return (double) num;
        Label_00DF:
            return string.Empty;
        }

        private XPathNavigator EvaluateArg(XPathNodeIterator context)
        {
            if (this.arg == null)
            {
                return context.Current;
            }
            this.arg.Evaluate(context);
            return this.arg.Advance();
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("name", this.funcType.ToString());
            if (this.arg != null)
            {
                this.arg.PrintQuery(w);
            }
            w.WriteEndElement();
        }

        public override void SetXsltContext(XsltContext context)
        {
            this.xsltContext = context.Whitespace ? context : null;
            if (this.arg != null)
            {
                this.arg.SetXsltContext(context);
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                return Function.ReturnTypes[(int) this.funcType];
            }
        }
    }
}

