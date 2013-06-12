namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class StringFunctions : ValueQuery
    {
        private IList<Query> argList;
        private static readonly CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        private Function.FunctionType funcType;

        private StringFunctions(StringFunctions other) : base(other)
        {
            this.funcType = other.funcType;
            Query[] queryArray = new Query[other.argList.Count];
            for (int i = 0; i < queryArray.Length; i++)
            {
                queryArray[i] = Query.Clone(other.argList[i]);
            }
            this.argList = queryArray;
        }

        public StringFunctions(Function.FunctionType funcType, IList<Query> argList)
        {
            this.funcType = funcType;
            this.argList = argList;
        }

        public override XPathNodeIterator Clone()
        {
            return new StringFunctions(this);
        }

        private string Concat(XPathNodeIterator nodeIterator)
        {
            int num = 0;
            StringBuilder builder = new StringBuilder();
            while (num < this.argList.Count)
            {
                builder.Append(this.argList[num++].Evaluate(nodeIterator).ToString());
            }
            return builder.ToString();
        }

        private bool Contains(XPathNodeIterator nodeIterator)
        {
            string source = this.argList[0].Evaluate(nodeIterator).ToString();
            string str2 = this.argList[1].Evaluate(nodeIterator).ToString();
            return (compareInfo.IndexOf(source, str2, CompareOptions.Ordinal) >= 0);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            switch (this.funcType)
            {
                case Function.FunctionType.FuncString:
                    return this.toString(nodeIterator);

                case Function.FunctionType.FuncConcat:
                    return this.Concat(nodeIterator);

                case Function.FunctionType.FuncStartsWith:
                    return this.StartsWith(nodeIterator);

                case Function.FunctionType.FuncContains:
                    return this.Contains(nodeIterator);

                case Function.FunctionType.FuncSubstringBefore:
                    return this.SubstringBefore(nodeIterator);

                case Function.FunctionType.FuncSubstringAfter:
                    return this.SubstringAfter(nodeIterator);

                case Function.FunctionType.FuncSubstring:
                    return this.Substring(nodeIterator);

                case Function.FunctionType.FuncStringLength:
                    return this.StringLength(nodeIterator);

                case Function.FunctionType.FuncNormalize:
                    return this.Normalize(nodeIterator);

                case Function.FunctionType.FuncTranslate:
                    return this.Translate(nodeIterator);
            }
            return string.Empty;
        }

        private string Normalize(XPathNodeIterator nodeIterator)
        {
            string str;
            if (this.argList.Count > 0)
            {
                str = this.argList[0].Evaluate(nodeIterator).ToString();
            }
            else
            {
                str = nodeIterator.Current.Value;
            }
            str = XmlConvert.TrimString(str);
            int num = 0;
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            XmlCharType instance = XmlCharType.Instance;
            while (num < str.Length)
            {
                if (!instance.IsWhiteSpace(str[num]))
                {
                    flag = true;
                    builder.Append(str[num]);
                }
                else if (flag)
                {
                    flag = false;
                    builder.Append(' ');
                }
                num++;
            }
            return builder.ToString();
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            w.WriteAttributeString("name", this.funcType.ToString());
            foreach (Query query in this.argList)
            {
                query.PrintQuery(w);
            }
            w.WriteEndElement();
        }

        public override void SetXsltContext(XsltContext context)
        {
            for (int i = 0; i < this.argList.Count; i++)
            {
                this.argList[i].SetXsltContext(context);
            }
        }

        private bool StartsWith(XPathNodeIterator nodeIterator)
        {
            string strA = this.argList[0].Evaluate(nodeIterator).ToString();
            string strB = this.argList[1].Evaluate(nodeIterator).ToString();
            return ((strA.Length >= strB.Length) && (string.CompareOrdinal(strA, 0, strB, 0, strB.Length) == 0));
        }

        private double StringLength(XPathNodeIterator nodeIterator)
        {
            if (this.argList.Count > 0)
            {
                return (double) this.argList[0].Evaluate(nodeIterator).ToString().Length;
            }
            return (double) nodeIterator.Current.Value.Length;
        }

        private string Substring(XPathNodeIterator nodeIterator)
        {
            string str = this.argList[0].Evaluate(nodeIterator).ToString();
            double d = XmlConvert.XPathRound(XmlConvert.ToXPathDouble(this.argList[1].Evaluate(nodeIterator))) - 1.0;
            if (double.IsNaN(d) || (str.Length <= d))
            {
                return string.Empty;
            }
            if (this.argList.Count == 3)
            {
                double num2 = XmlConvert.XPathRound(XmlConvert.ToXPathDouble(this.argList[2].Evaluate(nodeIterator)));
                if (double.IsNaN(num2))
                {
                    return string.Empty;
                }
                if ((d < 0.0) || (num2 < 0.0))
                {
                    num2 = d + num2;
                    if (num2 <= 0.0)
                    {
                        return string.Empty;
                    }
                    d = 0.0;
                }
                double num3 = str.Length - d;
                if (num2 > num3)
                {
                    num2 = num3;
                }
                return str.Substring((int) d, (int) num2);
            }
            if (d < 0.0)
            {
                d = 0.0;
            }
            return str.Substring((int) d);
        }

        private string SubstringAfter(XPathNodeIterator nodeIterator)
        {
            string source = this.argList[0].Evaluate(nodeIterator).ToString();
            string str2 = this.argList[1].Evaluate(nodeIterator).ToString();
            if (str2.Length == 0)
            {
                return source;
            }
            int num = compareInfo.IndexOf(source, str2, CompareOptions.Ordinal);
            if (num >= 0)
            {
                return source.Substring(num + str2.Length);
            }
            return string.Empty;
        }

        private string SubstringBefore(XPathNodeIterator nodeIterator)
        {
            string source = this.argList[0].Evaluate(nodeIterator).ToString();
            string str2 = this.argList[1].Evaluate(nodeIterator).ToString();
            if (str2.Length == 0)
            {
                return str2;
            }
            int length = compareInfo.IndexOf(source, str2, CompareOptions.Ordinal);
            if (length >= 1)
            {
                return source.Substring(0, length);
            }
            return string.Empty;
        }

        internal static string toString(bool b)
        {
            if (!b)
            {
                return "false";
            }
            return "true";
        }

        internal static string toString(double num)
        {
            return num.ToString("R", NumberFormatInfo.InvariantInfo);
        }

        private string toString(XPathNodeIterator nodeIterator)
        {
            if (this.argList.Count <= 0)
            {
                return nodeIterator.Current.Value;
            }
            object obj2 = this.argList[0].Evaluate(nodeIterator);
            switch (base.GetXPathType(obj2))
            {
                case XPathResultType.String:
                    return (string) obj2;

                case XPathResultType.Boolean:
                    if ((bool) obj2)
                    {
                        return "true";
                    }
                    return "false";

                case XPathResultType.NodeSet:
                {
                    XPathNavigator navigator = this.argList[0].Advance();
                    if (navigator != null)
                    {
                        return navigator.Value;
                    }
                    return string.Empty;
                }
                case ((XPathResultType) 4):
                    return ((XPathNavigator) obj2).Value;
            }
            return toString((double) obj2);
        }

        private string Translate(XPathNodeIterator nodeIterator)
        {
            string str = this.argList[0].Evaluate(nodeIterator).ToString();
            string str2 = this.argList[1].Evaluate(nodeIterator).ToString();
            string str3 = this.argList[2].Evaluate(nodeIterator).ToString();
            int num = 0;
            StringBuilder builder = new StringBuilder();
            while (num < str.Length)
            {
                int index = str2.IndexOf(str[num]);
                if (index != -1)
                {
                    if (index < str3.Length)
                    {
                        builder.Append(str3[index]);
                    }
                }
                else
                {
                    builder.Append(str[num]);
                }
                num++;
            }
            return builder.ToString();
        }

        public override XPathResultType StaticType
        {
            get
            {
                if (this.funcType == Function.FunctionType.FuncStringLength)
                {
                    return XPathResultType.Number;
                }
                if ((this.funcType != Function.FunctionType.FuncStartsWith) && (this.funcType != Function.FunctionType.FuncContains))
                {
                    return XPathResultType.String;
                }
                return XPathResultType.Boolean;
            }
        }
    }
}

