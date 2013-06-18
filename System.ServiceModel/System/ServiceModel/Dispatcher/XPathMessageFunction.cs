namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal abstract class XPathMessageFunction : IXsltContextFunction
    {
        private XPathResultType[] argTypes;
        private int maxArgs;
        private int minArgs;
        internal static readonly XmlNamespaceManager Namespaces = new XmlNamespaceManager(new NameTable());
        private XPathResultType retType;
        internal static readonly DateTime ZeroDate = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static XPathMessageFunction()
        {
            Namespaces.AddNamespace("s11", "http://schemas.xmlsoap.org/soap/envelope/");
            Namespaces.AddNamespace("s12", "http://www.w3.org/2003/05/soap-envelope");
        }

        protected XPathMessageFunction(XPathResultType[] argTypes, int max, int min, XPathResultType retType)
        {
            this.argTypes = argTypes;
            this.maxArgs = max;
            this.minArgs = min;
            this.retType = retType;
        }

        internal static double ConvertDate(DateTime date)
        {
            if (date.Kind != DateTimeKind.Utc)
            {
                date = date.ToUniversalTime();
            }
            TimeSpan span = (TimeSpan) (date - ZeroDate);
            return span.TotalDays;
        }

        public abstract object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext);
        internal abstract void InvokeInternal(ProcessingContext context, int argCount);
        internal static bool MoveToAddressingHeader(XPathNavigator nav, string name)
        {
            if (MoveToHeader(nav))
            {
                if (!nav.MoveToFirstChild())
                {
                    return false;
                }
                do
                {
                    if ((nav.LocalName == name) && (((nav.NamespaceURI == "http://www.w3.org/2005/08/addressing") || (nav.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/08/addressing")) || (nav.NamespaceURI == "http://schemas.microsoft.com/ws/2005/05/addressing/none")))
                    {
                        return true;
                    }
                }
                while (nav.MoveToNext());
            }
            return false;
        }

        internal static bool MoveToAddressingHeaderSibling(XPathNavigator nav, string name)
        {
            while (nav.MoveToNext())
            {
                if ((nav.LocalName == name) && ((nav.NamespaceURI == "http://www.w3.org/2005/08/addressing") || (nav.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/08/addressing")))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool MoveToBody(XPathNavigator nav)
        {
            nav.MoveToRoot();
            if (nav.MoveToFirstChild())
            {
                string namespaceURI = nav.NamespaceURI;
                if ((nav.LocalName != "Envelope") || ((namespaceURI != "http://schemas.xmlsoap.org/soap/envelope/") && (namespaceURI != "http://www.w3.org/2003/05/soap-envelope")))
                {
                    return false;
                }
                if (nav.MoveToFirstChild())
                {
                    do
                    {
                        if ((nav.LocalName == "Body") && (nav.NamespaceURI == namespaceURI))
                        {
                            return true;
                        }
                    }
                    while (nav.MoveToNext());
                }
            }
            return false;
        }

        internal static bool MoveToChild(XPathNavigator nav, string name, string ns)
        {
            if (nav.MoveToFirstChild())
            {
                do
                {
                    if ((nav.LocalName == name) && (nav.NamespaceURI == ns))
                    {
                        return true;
                    }
                }
                while (nav.MoveToNext());
            }
            return false;
        }

        internal static bool MoveToHeader(XPathNavigator nav)
        {
            nav.MoveToRoot();
            if (nav.MoveToFirstChild())
            {
                string namespaceURI = nav.NamespaceURI;
                if ((nav.LocalName != "Envelope") || ((namespaceURI != "http://schemas.xmlsoap.org/soap/envelope/") && (namespaceURI != "http://www.w3.org/2003/05/soap-envelope")))
                {
                    return false;
                }
                if (nav.MoveToFirstChild())
                {
                    do
                    {
                        if ((nav.LocalName == "Header") && (nav.NamespaceURI == namespaceURI))
                        {
                            return true;
                        }
                    }
                    while (nav.MoveToNext());
                }
            }
            return false;
        }

        internal static bool MoveToSibling(XPathNavigator nav, string name, string ns)
        {
            while (nav.MoveToNext())
            {
                if ((nav.LocalName == name) && (nav.NamespaceURI == ns))
                {
                    return true;
                }
            }
            return false;
        }

        internal static string ToString(object o)
        {
            if (o is bool)
            {
                return QueryValueModel.String((bool) o);
            }
            if (o is string)
            {
                return (string) o;
            }
            if (o is double)
            {
                return QueryValueModel.String((double) o);
            }
            if (!(o is XPathNodeIterator))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("QueryFunctionStringArg")));
            }
            XPathNodeIterator iterator = (XPathNodeIterator) o;
            iterator.MoveNext();
            return iterator.Current.Value;
        }

        public XPathResultType[] ArgTypes
        {
            get
            {
                return this.argTypes;
            }
        }

        public int Maxargs
        {
            get
            {
                return this.maxArgs;
            }
        }

        public int Minargs
        {
            get
            {
                return this.minArgs;
            }
        }

        public XPathResultType ReturnType
        {
            get
            {
                return this.retType;
            }
        }
    }
}

