namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class XPathComparerHelper : IComparer
    {
        private XmlCaseOrder caseOrder;
        private CultureInfo cinfo;
        private XmlDataType dataType;
        private XmlSortOrder order;

        public XPathComparerHelper(XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType)
        {
            if (lang == null)
            {
                this.cinfo = Thread.CurrentThread.CurrentCulture;
            }
            else
            {
                try
                {
                    this.cinfo = new CultureInfo(lang);
                }
                catch (ArgumentException)
                {
                    throw;
                }
            }
            if (order == XmlSortOrder.Descending)
            {
                if (caseOrder == XmlCaseOrder.LowerFirst)
                {
                    caseOrder = XmlCaseOrder.UpperFirst;
                }
                else if (caseOrder == XmlCaseOrder.UpperFirst)
                {
                    caseOrder = XmlCaseOrder.LowerFirst;
                }
            }
            this.order = order;
            this.caseOrder = caseOrder;
            this.dataType = dataType;
        }

        public int Compare(object x, object y)
        {
            int num;
            switch (this.dataType)
            {
                case XmlDataType.Text:
                {
                    string strA = Convert.ToString(x, this.cinfo);
                    string strB = Convert.ToString(y, this.cinfo);
                    num = string.Compare(strA, strB, this.caseOrder != XmlCaseOrder.None, this.cinfo);
                    if ((num == 0) && (this.caseOrder != XmlCaseOrder.None))
                    {
                        num = string.Compare(strA, strB, false, this.cinfo);
                        if (this.caseOrder != XmlCaseOrder.LowerFirst)
                        {
                            return -num;
                        }
                        return num;
                    }
                    if (this.order != XmlSortOrder.Ascending)
                    {
                        return -num;
                    }
                    return num;
                }
                case XmlDataType.Number:
                {
                    double num2 = XmlConvert.ToXPathDouble(x);
                    double num3 = XmlConvert.ToXPathDouble(y);
                    num = num2.CompareTo(num3);
                    if (this.order == XmlSortOrder.Ascending)
                    {
                        return num;
                    }
                    return -num;
                }
            }
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }
    }
}

