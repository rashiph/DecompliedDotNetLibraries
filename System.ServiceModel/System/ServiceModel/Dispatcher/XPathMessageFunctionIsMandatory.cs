namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionIsMandatory : XPathMessageFunction
    {
        internal XPathMessageFunctionIsMandatory() : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.Boolean)
        {
        }

        internal static bool ExtractFromNavigator(XPathNavigator nav)
        {
            string attribute = nav.GetAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");
            string str2 = nav.GetAttribute("mustUnderstand", "http://www.w3.org/2003/05/soap-envelope");
            nav.MoveToRoot();
            nav.MoveToFirstChild();
            if ((nav.LocalName == "Envelope") && (nav.NamespaceURI == "http://schemas.xmlsoap.org/soap/envelope/"))
            {
                return (attribute == "1");
            }
            return (((nav.LocalName == "Envelope") && (nav.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope")) && (str2 == "true"));
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            XPathNodeIterator iterator = (XPathNodeIterator) args[0];
            if (!iterator.MoveNext())
            {
                return false;
            }
            return ExtractFromNavigator(iterator.Current.Clone());
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                bool val = false;
                NodeSequence sequence = context.PeekSequence(topArg.basePtr);
                if (sequence.Count > 0)
                {
                    NodeSequenceItem item = sequence[0];
                    SeekableXPathNavigator node = item.Node.Node;
                    long currentPosition = node.CurrentPosition;
                    NodeSequenceItem item2 = sequence[0];
                    node.CurrentPosition = item2.Node.Position;
                    val = ExtractFromNavigator(node);
                    node.CurrentPosition = currentPosition;
                }
                context.SetValue(context, topArg.basePtr, val);
                topArg.basePtr++;
            }
        }
    }
}

