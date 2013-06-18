namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    public sealed class XPathBinder
    {
        private XPathBinder()
        {
        }

        public static object Eval(object container, string xPath)
        {
            IXmlNamespaceResolver resolver = null;
            return Eval(container, xPath, resolver);
        }

        public static string Eval(object container, string xPath, string format)
        {
            return Eval(container, xPath, format, null);
        }

        public static object Eval(object container, string xPath, IXmlNamespaceResolver resolver)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (string.IsNullOrEmpty(xPath))
            {
                throw new ArgumentNullException("xPath");
            }
            IXPathNavigable navigable = container as IXPathNavigable;
            if (navigable == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("XPathBinder_MustBeIXPathNavigable", new object[] { container.GetType().FullName }));
            }
            object obj2 = navigable.CreateNavigator().Evaluate(xPath, resolver);
            XPathNodeIterator iterator = obj2 as XPathNodeIterator;
            if (iterator == null)
            {
                return obj2;
            }
            if (iterator.MoveNext())
            {
                return iterator.Current.Value;
            }
            return null;
        }

        public static string Eval(object container, string xPath, string format, IXmlNamespaceResolver resolver)
        {
            object obj2 = Eval(container, xPath, resolver);
            if (obj2 == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(format))
            {
                return obj2.ToString();
            }
            return string.Format(format, obj2);
        }

        public static IEnumerable Select(object container, string xPath)
        {
            return Select(container, xPath, null);
        }

        public static IEnumerable Select(object container, string xPath, IXmlNamespaceResolver resolver)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (string.IsNullOrEmpty(xPath))
            {
                throw new ArgumentNullException("xPath");
            }
            ArrayList list = new ArrayList();
            IXPathNavigable navigable = container as IXPathNavigable;
            if (navigable == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("XPathBinder_MustBeIXPathNavigable", new object[] { container.GetType().FullName }));
            }
            XPathNodeIterator iterator = navigable.CreateNavigator().Select(xPath, resolver);
            while (iterator.MoveNext())
            {
                IHasXmlNode current = iterator.Current as IHasXmlNode;
                if (current == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XPathBinder_MustHaveXmlNodes"));
                }
                list.Add(current.GetNode());
            }
            return list;
        }
    }
}

