namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class LogicalExpr : ValueQuery
    {
        private static readonly cmpXslt[][] CompXsltE;
        private static readonly cmpXslt[][] CompXsltO;
        private Operator.Op op;
        private Query opnd1;
        private Query opnd2;

        static LogicalExpr()
        {
            cmpXslt[][] xsltArray = new cmpXslt[5][];
            cmpXslt[] xsltArray2 = new cmpXslt[5];
            xsltArray2[0] = new cmpXslt(LogicalExpr.cmpNumberNumber);
            xsltArray[0] = xsltArray2;
            cmpXslt[] xsltArray3 = new cmpXslt[5];
            xsltArray3[0] = new cmpXslt(LogicalExpr.cmpStringNumber);
            xsltArray3[1] = new cmpXslt(LogicalExpr.cmpStringStringE);
            xsltArray[1] = xsltArray3;
            cmpXslt[] xsltArray4 = new cmpXslt[5];
            xsltArray4[0] = new cmpXslt(LogicalExpr.cmpBoolNumberE);
            xsltArray4[1] = new cmpXslt(LogicalExpr.cmpBoolStringE);
            xsltArray4[2] = new cmpXslt(LogicalExpr.cmpBoolBoolE);
            xsltArray[2] = xsltArray4;
            cmpXslt[] xsltArray5 = new cmpXslt[5];
            xsltArray5[0] = new cmpXslt(LogicalExpr.cmpQueryNumber);
            xsltArray5[1] = new cmpXslt(LogicalExpr.cmpQueryStringE);
            xsltArray5[2] = new cmpXslt(LogicalExpr.cmpQueryBoolE);
            xsltArray5[3] = new cmpXslt(LogicalExpr.cmpQueryQueryE);
            xsltArray[3] = xsltArray5;
            xsltArray[4] = new cmpXslt[] { new cmpXslt(LogicalExpr.cmpRtfNumber), new cmpXslt(LogicalExpr.cmpRtfStringE), new cmpXslt(LogicalExpr.cmpRtfBoolE), new cmpXslt(LogicalExpr.cmpRtfQueryE), new cmpXslt(LogicalExpr.cmpRtfRtfE) };
            CompXsltE = xsltArray;
            cmpXslt[][] xsltArray7 = new cmpXslt[5][];
            cmpXslt[] xsltArray8 = new cmpXslt[5];
            xsltArray8[0] = new cmpXslt(LogicalExpr.cmpNumberNumber);
            xsltArray7[0] = xsltArray8;
            cmpXslt[] xsltArray9 = new cmpXslt[5];
            xsltArray9[0] = new cmpXslt(LogicalExpr.cmpStringNumber);
            xsltArray9[1] = new cmpXslt(LogicalExpr.cmpStringStringO);
            xsltArray7[1] = xsltArray9;
            cmpXslt[] xsltArray10 = new cmpXslt[5];
            xsltArray10[0] = new cmpXslt(LogicalExpr.cmpBoolNumberO);
            xsltArray10[1] = new cmpXslt(LogicalExpr.cmpBoolStringO);
            xsltArray10[2] = new cmpXslt(LogicalExpr.cmpBoolBoolO);
            xsltArray7[2] = xsltArray10;
            cmpXslt[] xsltArray11 = new cmpXslt[5];
            xsltArray11[0] = new cmpXslt(LogicalExpr.cmpQueryNumber);
            xsltArray11[1] = new cmpXslt(LogicalExpr.cmpQueryStringO);
            xsltArray11[2] = new cmpXslt(LogicalExpr.cmpQueryBoolO);
            xsltArray11[3] = new cmpXslt(LogicalExpr.cmpQueryQueryO);
            xsltArray7[3] = xsltArray11;
            xsltArray7[4] = new cmpXslt[] { new cmpXslt(LogicalExpr.cmpRtfNumber), new cmpXslt(LogicalExpr.cmpRtfStringO), new cmpXslt(LogicalExpr.cmpRtfBoolO), new cmpXslt(LogicalExpr.cmpRtfQueryO), new cmpXslt(LogicalExpr.cmpRtfRtfO) };
            CompXsltO = xsltArray7;
        }

        private LogicalExpr(LogicalExpr other) : base(other)
        {
            this.op = other.op;
            this.opnd1 = Query.Clone(other.opnd1);
            this.opnd2 = Query.Clone(other.opnd2);
        }

        public LogicalExpr(Operator.Op op, Query opnd1, Query opnd2)
        {
            this.op = op;
            this.opnd1 = opnd1;
            this.opnd2 = opnd2;
        }

        public override XPathNodeIterator Clone()
        {
            return new LogicalExpr(this);
        }

        private static bool cmpBoolBoolE(Operator.Op op, bool n1, bool n2)
        {
            return ((op == Operator.Op.EQ) == (n1 == n2));
        }

        private static bool cmpBoolBoolE(Operator.Op op, object val1, object val2)
        {
            bool flag = (bool) val1;
            bool flag2 = (bool) val2;
            return cmpBoolBoolE(op, flag, flag2);
        }

        private static bool cmpBoolBoolO(Operator.Op op, object val1, object val2)
        {
            double num = NumberFunctions.Number((bool) val1);
            double num2 = NumberFunctions.Number((bool) val2);
            return cmpNumberNumberO(op, num, num2);
        }

        private static bool cmpBoolNumberE(Operator.Op op, object val1, object val2)
        {
            bool flag = (bool) val1;
            bool flag2 = BooleanFunctions.toBoolean((double) val2);
            return cmpBoolBoolE(op, flag, flag2);
        }

        private static bool cmpBoolNumberO(Operator.Op op, object val1, object val2)
        {
            double num = NumberFunctions.Number((bool) val1);
            double num2 = (double) val2;
            return cmpNumberNumberO(op, num, num2);
        }

        private static bool cmpBoolStringE(Operator.Op op, object val1, object val2)
        {
            bool flag = (bool) val1;
            bool flag2 = BooleanFunctions.toBoolean((string) val2);
            return cmpBoolBoolE(op, flag, flag2);
        }

        private static bool cmpBoolStringO(Operator.Op op, object val1, object val2)
        {
            return cmpNumberNumberO(op, NumberFunctions.Number((bool) val1), NumberFunctions.Number((string) val2));
        }

        private static bool cmpNumberNumber(Operator.Op op, double n1, double n2)
        {
            switch (op)
            {
                case Operator.Op.EQ:
                    return (n1 == n2);

                case Operator.Op.NE:
                    return !(n1 == n2);

                case Operator.Op.LT:
                    return (n1 < n2);

                case Operator.Op.LE:
                    return (n1 <= n2);

                case Operator.Op.GT:
                    return (n1 > n2);

                case Operator.Op.GE:
                    return (n1 >= n2);
            }
            return false;
        }

        private static bool cmpNumberNumber(Operator.Op op, object val1, object val2)
        {
            double num = (double) val1;
            double num2 = (double) val2;
            return cmpNumberNumber(op, num, num2);
        }

        private static bool cmpNumberNumberO(Operator.Op op, double n1, double n2)
        {
            switch (op)
            {
                case Operator.Op.LT:
                    return (n1 < n2);

                case Operator.Op.LE:
                    return (n1 <= n2);

                case Operator.Op.GT:
                    return (n1 > n2);

                case Operator.Op.GE:
                    return (n1 >= n2);
            }
            return false;
        }

        private static bool cmpQueryBoolE(Operator.Op op, object val1, object val2)
        {
            bool flag = new NodeSet(val1).MoveNext();
            bool flag2 = (bool) val2;
            return cmpBoolBoolE(op, flag, flag2);
        }

        private static bool cmpQueryBoolO(Operator.Op op, object val1, object val2)
        {
            NodeSet set = new NodeSet(val1);
            double num = set.MoveNext() ? 1.0 : 0.0;
            double num2 = NumberFunctions.Number((bool) val2);
            return cmpNumberNumberO(op, num, num2);
        }

        private static bool cmpQueryNumber(Operator.Op op, object val1, object val2)
        {
            NodeSet set = new NodeSet(val1);
            double num = (double) val2;
            while (set.MoveNext())
            {
                if (cmpNumberNumber(op, NumberFunctions.Number(set.Value), num))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool cmpQueryQueryE(Operator.Op op, object val1, object val2)
        {
            bool flag = op == Operator.Op.EQ;
            NodeSet set = new NodeSet(val1);
            NodeSet set2 = new NodeSet(val2);
            while (set.MoveNext())
            {
                if (!set2.MoveNext())
                {
                    return false;
                }
                string str = set.Value;
                do
                {
                    if ((str == set2.Value) == flag)
                    {
                        return true;
                    }
                }
                while (set2.MoveNext());
                set2.Reset();
            }
            return false;
        }

        private static bool cmpQueryQueryO(Operator.Op op, object val1, object val2)
        {
            NodeSet set = new NodeSet(val1);
            NodeSet set2 = new NodeSet(val2);
            while (set.MoveNext())
            {
                if (!set2.MoveNext())
                {
                    return false;
                }
                double num = NumberFunctions.Number(set.Value);
                do
                {
                    if (cmpNumberNumber(op, num, NumberFunctions.Number(set2.Value)))
                    {
                        return true;
                    }
                }
                while (set2.MoveNext());
                set2.Reset();
            }
            return false;
        }

        private static bool cmpQueryStringE(Operator.Op op, object val1, object val2)
        {
            NodeSet set = new NodeSet(val1);
            string str = (string) val2;
            while (set.MoveNext())
            {
                if (cmpStringStringE(op, set.Value, str))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool cmpQueryStringO(Operator.Op op, object val1, object val2)
        {
            NodeSet set = new NodeSet(val1);
            double num = NumberFunctions.Number((string) val2);
            while (set.MoveNext())
            {
                if (cmpNumberNumberO(op, NumberFunctions.Number(set.Value), num))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool cmpRtfBoolE(Operator.Op op, object val1, object val2)
        {
            bool flag = BooleanFunctions.toBoolean(Rtf(val1));
            bool flag2 = (bool) val2;
            return cmpBoolBoolE(op, flag, flag2);
        }

        private static bool cmpRtfBoolO(Operator.Op op, object val1, object val2)
        {
            return cmpNumberNumberO(op, NumberFunctions.Number(Rtf(val1)), NumberFunctions.Number((bool) val2));
        }

        private static bool cmpRtfNumber(Operator.Op op, object val1, object val2)
        {
            double num = (double) val2;
            double num2 = NumberFunctions.Number(Rtf(val1));
            return cmpNumberNumber(op, num2, num);
        }

        private static bool cmpRtfQueryE(Operator.Op op, object val1, object val2)
        {
            string str = Rtf(val1);
            NodeSet set = new NodeSet(val2);
            while (set.MoveNext())
            {
                if (cmpStringStringE(op, str, set.Value))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool cmpRtfQueryO(Operator.Op op, object val1, object val2)
        {
            double num = NumberFunctions.Number(Rtf(val1));
            NodeSet set = new NodeSet(val2);
            while (set.MoveNext())
            {
                if (cmpNumberNumberO(op, num, NumberFunctions.Number(set.Value)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool cmpRtfRtfE(Operator.Op op, object val1, object val2)
        {
            string str = Rtf(val1);
            string str2 = Rtf(val2);
            return cmpStringStringE(op, str, str2);
        }

        private static bool cmpRtfRtfO(Operator.Op op, object val1, object val2)
        {
            double num = NumberFunctions.Number(Rtf(val1));
            double num2 = NumberFunctions.Number(Rtf(val2));
            return cmpNumberNumberO(op, num, num2);
        }

        private static bool cmpRtfStringE(Operator.Op op, object val1, object val2)
        {
            string str = Rtf(val1);
            string str2 = (string) val2;
            return cmpStringStringE(op, str, str2);
        }

        private static bool cmpRtfStringO(Operator.Op op, object val1, object val2)
        {
            double num = NumberFunctions.Number(Rtf(val1));
            double num2 = NumberFunctions.Number((string) val2);
            return cmpNumberNumberO(op, num, num2);
        }

        private static bool cmpStringNumber(Operator.Op op, object val1, object val2)
        {
            double num = (double) val2;
            double num2 = NumberFunctions.Number((string) val1);
            return cmpNumberNumber(op, num2, num);
        }

        private static bool cmpStringStringE(Operator.Op op, object val1, object val2)
        {
            string str = (string) val1;
            string str2 = (string) val2;
            return cmpStringStringE(op, str, str2);
        }

        private static bool cmpStringStringE(Operator.Op op, string n1, string n2)
        {
            return ((op == Operator.Op.EQ) == (n1 == n2));
        }

        private static bool cmpStringStringO(Operator.Op op, object val1, object val2)
        {
            double num = NumberFunctions.Number((string) val1);
            double num2 = NumberFunctions.Number((string) val2);
            return cmpNumberNumberO(op, num, num2);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            Operator.Op op = this.op;
            object obj2 = this.opnd1.Evaluate(nodeIterator);
            object obj3 = this.opnd2.Evaluate(nodeIterator);
            int xPathType = (int) base.GetXPathType(obj2);
            int index = (int) base.GetXPathType(obj3);
            if (xPathType < index)
            {
                op = Operator.InvertOperator(op);
                object obj4 = obj2;
                obj2 = obj3;
                obj3 = obj4;
                int num3 = xPathType;
                xPathType = index;
                index = num3;
            }
            if ((op != Operator.Op.EQ) && (op != Operator.Op.NE))
            {
                return CompXsltO[xPathType][index](op, obj2, obj3);
            }
            return CompXsltE[xPathType][index](op, obj2, obj3);
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("op", this.op.ToString());
            this.opnd1.PrintQuery(w);
            this.opnd2.PrintQuery(w);
            w.WriteEndElement();
        }

        private static string Rtf(object o)
        {
            return ((XPathNavigator) o).Value;
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
                return XPathResultType.Boolean;
            }
        }

        private delegate bool cmpXslt(Operator.Op op, object val1, object val2);

        [StructLayout(LayoutKind.Sequential)]
        private struct NodeSet
        {
            private Query opnd;
            private XPathNavigator current;
            public NodeSet(object opnd)
            {
                this.opnd = (Query) opnd;
                this.current = null;
            }

            public bool MoveNext()
            {
                this.current = this.opnd.Advance();
                return (this.current != null);
            }

            public void Reset()
            {
                this.opnd.Reset();
            }

            public string Value
            {
                get
                {
                    return this.current.Value;
                }
            }
        }
    }
}

