namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionActor : XPathMessageFunction
    {
        internal XPathMessageFunctionActor() : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.String)
        {
        }

        internal static string ExtractFromNavigator(XPathNavigator nav)
        {
            string attribute = nav.GetAttribute(XPathMessageContext.Actor11A, "http://schemas.xmlsoap.org/soap/envelope/");
            string str2 = nav.GetAttribute(XPathMessageContext.Actor12A, "http://www.w3.org/2003/05/soap-envelope");
            nav.MoveToRoot();
            nav.MoveToFirstChild();
            if ((nav.LocalName == "Envelope") && (nav.NamespaceURI == "http://schemas.xmlsoap.org/soap/envelope/"))
            {
                return attribute;
            }
            if ((nav.LocalName == "Envelope") && (nav.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope"))
            {
                return str2;
            }
            return string.Empty;
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            XPathNodeIterator iterator = (XPathNodeIterator) args[0];
            if (!iterator.MoveNext())
            {
                return string.Empty;
            }
            return ExtractFromNavigator(iterator.Current.Clone());
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string val = string.Empty;
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

