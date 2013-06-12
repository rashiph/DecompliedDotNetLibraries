namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class NumberFunctions : ValueQuery
    {
        private Query arg;
        private Function.FunctionType ftype;

        private NumberFunctions(NumberFunctions other) : base(other)
        {
            this.arg = Query.Clone(other.arg);
            this.ftype = other.ftype;
        }

        public NumberFunctions(Function.FunctionType ftype, Query arg)
        {
            this.arg = arg;
            this.ftype = ftype;
        }

        private double Ceiling(XPathNodeIterator nodeIterator)
        {
            return Math.Ceiling((double) this.arg.Evaluate(nodeIterator));
        }

        public override XPathNodeIterator Clone()
        {
            return new NumberFunctions(this);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            switch (this.ftype)
            {
                case Function.FunctionType.FuncSum:
                    return this.Sum(nodeIterator);

                case Function.FunctionType.FuncFloor:
                    return this.Floor(nodeIterator);

                case Function.FunctionType.FuncCeiling:
                    return this.Ceiling(nodeIterator);

                case Function.FunctionType.FuncRound:
                    return this.Round(nodeIterator);

                case Function.FunctionType.FuncNumber:
                    return this.Number(nodeIterator);
            }
            return null;
        }

        private double Floor(XPathNodeIterator nodeIterator)
        {
            return Math.Floor((double) this.arg.Evaluate(nodeIterator));
        }

        internal static double Number(bool arg)
        {
            if (!arg)
            {
                return 0.0;
            }
            return 1.0;
        }

        internal static double Number(string arg)
        {
            return XmlConvert.ToXPathDouble(arg);
        }

        private double Number(XPathNodeIterator nodeIterator)
        {
            if (this.arg == null)
            {
                return XmlConvert.ToXPathDouble(nodeIterator.Current.Value);
            }
            object obj2 = this.arg.Evaluate(nodeIterator);
            switch (base.GetXPathType(obj2))
            {
                case XPathResultType.Number:
                    return (double) obj2;

                case XPathResultType.String:
                    return Number((string) obj2);

                case XPathResultType.Boolean:
                    return Number((bool) obj2);

                case XPathResultType.NodeSet:
                {
                    XPathNavigator navigator = this.arg.Advance();
                    if (navigator == null)
                    {
                        break;
                    }
                    return Number(navigator.Value);
                }
                case ((XPathResultType) 4):
                    return Number(((XPathNavigator) obj2).Value);
            }
            return double.NaN;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("name", this.ftype.ToString());
            if (this.arg != null)
            {
                this.arg.PrintQuery(w);
            }
            w.WriteEndElement();
        }

        private double Round(XPathNodeIterator nodeIterator)
        {
            return XmlConvert.XPathRound(XmlConvert.ToXPathDouble(this.arg.Evaluate(nodeIterator)));
        }

        public override void SetXsltContext(XsltContext context)
        {
            if (this.arg != null)
            {
                this.arg.SetXsltContext(context);
            }
        }

        private double Sum(XPathNodeIterator nodeIterator)
        {
            XPathNavigator navigator;
            double num = 0.0;
            this.arg.Evaluate(nodeIterator);
            while ((navigator = this.arg.Advance()) != null)
            {
                num += Number(navigator.Value);
            }
            return num;
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.Number;
            }
        }
    }
}

