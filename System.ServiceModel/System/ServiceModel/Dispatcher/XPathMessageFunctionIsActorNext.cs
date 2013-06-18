namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionIsActorNext : XPathMessageFunction
    {
        private static string S11Next = EnvelopeVersion.Soap11.NextDestinationActorValue;
        private static string S12Next = EnvelopeVersion.Soap12.NextDestinationActorValue;

        internal XPathMessageFunctionIsActorNext() : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.Boolean)
        {
        }

        internal static bool ExtractFromNavigator(XPathNavigator nav)
        {
            string str = XPathMessageFunctionActor.ExtractFromNavigator(nav);
            if (str.Length != 0)
            {
                nav.MoveToRoot();
                if (!nav.MoveToFirstChild())
                {
                    return false;
                }
                if (nav.LocalName == "Envelope")
                {
                    if (nav.NamespaceURI == "http://schemas.xmlsoap.org/soap/envelope/")
                    {
                        return (str == S11Next);
                    }
                    if (nav.NamespaceURI == "http://www.w3.org/2003/05/soap-envelope")
                    {
                        return (str == S12Next);
                    }
                }
            }
            return false;
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator nav)
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

