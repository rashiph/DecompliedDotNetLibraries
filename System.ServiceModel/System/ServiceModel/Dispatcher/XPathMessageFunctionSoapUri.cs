namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionSoapUri : XPathMessageFunction
    {
        public XPathMessageFunctionSoapUri() : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        internal static string ExtractFromNavigator(XPathNavigator nav)
        {
            nav.MoveToRoot();
            if (nav.MoveToFirstChild())
            {
                string namespaceURI = nav.NamespaceURI;
                if (!(nav.LocalName != "Envelope") && (!(namespaceURI != "http://schemas.xmlsoap.org/soap/envelope/") || !(namespaceURI != "http://www.w3.org/2003/05/soap-envelope")))
                {
                    return namespaceURI;
                }
            }
            return string.Empty;
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator navigator = docContext as SeekableMessageNavigator;
            if (navigator != null)
            {
                return navigator.Message.Version.Envelope.Namespace;
            }
            return ExtractFromNavigator(docContext.Clone());
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                string soapUri = context.Processor.SoapUri;
                if (soapUri == null)
                {
                    Message contextMessage = context.Processor.ContextMessage;
                    if (contextMessage == null)
                    {
                        SeekableXPathNavigator contextNode = context.Processor.ContextNode;
                        long currentPosition = contextNode.CurrentPosition;
                        soapUri = ExtractFromNavigator(contextNode);
                        contextNode.CurrentPosition = currentPosition;
                    }
                    else
                    {
                        soapUri = contextMessage.Version.Envelope.Namespace;
                    }
                    context.Processor.SoapUri = soapUri;
                }
                context.Push(soapUri, iterationCount);
            }
        }
    }
}

