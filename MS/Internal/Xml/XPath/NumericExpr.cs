namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class NumericExpr : ValueQuery
    {
        private Operator.Op op;
        private Query opnd1;
        private Query opnd2;

        private NumericExpr(NumericExpr other) : base(other)
        {
            this.op = other.op;
            this.opnd1 = Query.Clone(other.opnd1);
            this.opnd2 = Query.Clone(other.opnd2);
        }

        public NumericExpr(Operator.Op op, Query opnd1, Query opnd2)
        {
            if (opnd1.StaticType != XPathResultType.Number)
            {
                opnd1 = new NumberFunctions(Function.FunctionType.FuncNumber, opnd1);
            }
            if (opnd2.StaticType != XPathResultType.Number)
            {
                opnd2 = new NumberFunctions(Function.FunctionType.FuncNumber, opnd2);
            }
            this.op = op;
            this.opnd1 = opnd1;
            this.opnd2 = opnd2;
        }

        public override XPathNodeIterator Clone()
        {
            return new NumericExpr(this);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            return GetValue(this.op, XmlConvert.ToXPathDouble(this.opnd1.Evaluate(nodeIterator)), XmlConvert.ToXPathDouble(this.opnd2.Evaluate(nodeIterator)));
        }

        private static double GetValue(Operator.Op op, double n1, double n2)
        {
            switch (op)
            {
                case Operator.Op.PLUS:
                    return (n1 + n2);

                case Operator.Op.MINUS:
                    return (n1 - n2);

                case Operator.Op.MUL:
                    return (n1 * n2);

                case Operator.Op.DIV:
                    return (n1 / n2);

                case Operator.Op.MOD:
                    return (n1 % n2);
            }
            return 0.0;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("op", this.op.ToString());
            this.opnd1.PrintQuery(w);
            this.opnd2.PrintQuery(w);
            w.WriteEndElement();
        }

        public override void SetXsltContext(XsltContext context)
        {
            this.opnd1.SetXsltContext(context);
            this.opnd2.SetXsltContext(context);
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

