namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class BooleanFunctions : ValueQuery
    {
        private Query arg;
        private Function.FunctionType funcType;

        private BooleanFunctions(BooleanFunctions other) : base(other)
        {
            this.arg = Query.Clone(other.arg);
            this.funcType = other.funcType;
        }

        public BooleanFunctions(Function.FunctionType funcType, Query arg)
        {
            this.arg = arg;
            this.funcType = funcType;
        }

        public override XPathNodeIterator Clone()
        {
            return new BooleanFunctions(this);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            switch (this.funcType)
            {
                case Function.FunctionType.FuncBoolean:
                    return this.toBoolean(nodeIterator);

                case Function.FunctionType.FuncTrue:
                    return true;

                case Function.FunctionType.FuncFalse:
                    return false;

                case Function.FunctionType.FuncNot:
                    return this.Not(nodeIterator);

                case Function.FunctionType.FuncLang:
                    return this.Lang(nodeIterator);
            }
            return false;
        }

        private bool Lang(XPathNodeIterator nodeIterator)
        {
            string str = this.arg.Evaluate(nodeIterator).ToString();
            string xmlLang = nodeIterator.Current.XmlLang;
            if (!xmlLang.StartsWith(str, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (xmlLang.Length != str.Length)
            {
                return (xmlLang[str.Length] == '-');
            }
            return true;
        }

        private bool Not(XPathNodeIterator nodeIterator)
        {
            return !((bool) this.arg.Evaluate(nodeIterator));
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
            if (this.arg != null)
            {
                this.arg.SetXsltContext(context);
            }
        }

        internal static bool toBoolean(double number)
        {
            return ((number != 0.0) && !double.IsNaN(number));
        }

        internal static bool toBoolean(string str)
        {
            return (str.Length > 0);
        }

        internal bool toBoolean(XPathNodeIterator nodeIterator)
        {
            object obj2 = this.arg.Evaluate(nodeIterator);
            if (obj2 is XPathNodeIterator)
            {
                return (this.arg.Advance() != null);
            }
            if (obj2 is string)
            {
                return toBoolean((string) obj2);
            }
            if (obj2 is double)
            {
                return toBoolean((double) obj2);
            }
            if (obj2 is bool)
            {
                return (bool) obj2;
            }
            return true;
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.Boolean;
            }
        }
    }
}

