namespace System.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using System.Xml.Linq;

    public static class Extensions
    {
        private static XText CalibrateText(XText n)
        {
            if (n.parent == null)
            {
                return n;
            }
            XNode content = (XNode) n.parent.content;
            while (true)
            {
                content = content.next;
                XText text = content as XText;
                if (text != null)
                {
                    do
                    {
                        if (content == n)
                        {
                            return text;
                        }
                        content = content.next;
                    }
                    while (content is XText);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XPathNavigator CreateNavigator(this XNode node)
        {
            return node.CreateNavigator(null);
        }

        public static XPathNavigator CreateNavigator(this XNode node, XmlNameTable nameTable)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (node is XDocumentType)
            {
                throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_CreateNavigator", new object[] { XmlNodeType.DocumentType }));
            }
            XText n = node as XText;
            if (n != null)
            {
                if (n.parent is XDocument)
                {
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_CreateNavigator", new object[] { XmlNodeType.Whitespace }));
                }
                node = CalibrateText(n);
            }
            return new XNodeNavigator(node, nameTable);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object XPathEvaluate(this XNode node, string expression)
        {
            return node.XPathEvaluate(expression, null);
        }

        public static object XPathEvaluate(this XNode node, string expression, IXmlNamespaceResolver resolver)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            XPathEvaluator evaluator2 = new XPathEvaluator();
            return evaluator2.Evaluate<object>(node, expression, resolver);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static XElement XPathSelectElement(this XNode node, string expression)
        {
            return node.XPathSelectElement(expression, null);
        }

        public static XElement XPathSelectElement(this XNode node, string expression, IXmlNamespaceResolver resolver)
        {
            return node.XPathSelectElements(expression, resolver).FirstOrDefault<XElement>();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static IEnumerable<XElement> XPathSelectElements(this XNode node, string expression)
        {
            return node.XPathSelectElements(expression, null);
        }

        public static IEnumerable<XElement> XPathSelectElements(this XNode node, string expression, IXmlNamespaceResolver resolver)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            XPathEvaluator evaluator2 = new XPathEvaluator();
            return (IEnumerable<XElement>) evaluator2.Evaluate<XElement>(node, expression, resolver);
        }
    }
}

