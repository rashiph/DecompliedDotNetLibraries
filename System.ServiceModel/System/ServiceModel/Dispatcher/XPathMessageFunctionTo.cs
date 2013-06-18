namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionTo : XPathMessageFunction
    {
        public XPathMessageFunctionTo() : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        private static string ExtractFromNavigator(XPathNavigator nav)
        {
            if (!XPathMessageFunction.MoveToAddressingHeader(nav, "To"))
            {
                return string.Empty;
            }
            return nav.Value;
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator navigator = docContext as SeekableMessageNavigator;
            if (navigator == null)
            {
                return ExtractFromNavigator(docContext.Clone());
            }
            Uri to = navigator.Message.Headers.To;
            if (to == null)
            {
                return string.Empty;
            }
            return to.ToString();
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                string toHeader = context.Processor.ToHeader;
                if (toHeader == null)
                {
                    Message contextMessage = context.Processor.ContextMessage;
                    if (contextMessage == null)
                    {
                        SeekableXPathNavigator contextNode = context.Processor.ContextNode;
                        long currentPosition = contextNode.CurrentPosition;
                        toHeader = ExtractFromNavigator(contextNode);
                        contextNode.CurrentPosition = currentPosition;
                    }
                    else
                    {
                        Uri to = contextMessage.Headers.To;
                        if (to == null)
                        {
                            toHeader = contextMessage.Version.Addressing.Anonymous;
                        }
                        else
                        {
                            toHeader = to.AbsoluteUri;
                        }
                    }
                    context.Processor.ToHeader = toHeader;
                }
                context.Push(toHeader, iterationCount);
            }
        }
    }
}

