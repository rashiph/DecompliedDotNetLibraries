namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionMessageID : XPathMessageFunction
    {
        public XPathMessageFunctionMessageID() : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        private static string ExtractFromNavigator(XPathNavigator nav)
        {
            if (!XPathMessageFunction.MoveToAddressingHeader(nav, "MessageID"))
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
            UniqueId messageId = navigator.Message.Headers.MessageId;
            if (messageId == null)
            {
                return string.Empty;
            }
            return messageId.ToString();
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                string messageId = context.Processor.MessageId;
                if (messageId == null)
                {
                    Message contextMessage = context.Processor.ContextMessage;
                    if (contextMessage == null)
                    {
                        SeekableXPathNavigator contextNode = context.Processor.ContextNode;
                        long currentPosition = contextNode.CurrentPosition;
                        messageId = ExtractFromNavigator(contextNode);
                        contextNode.CurrentPosition = currentPosition;
                    }
                    else
                    {
                        UniqueId id = contextMessage.Headers.MessageId;
                        if (id == null)
                        {
                            messageId = string.Empty;
                        }
                        else
                        {
                            messageId = id.ToString();
                        }
                    }
                    context.Processor.MessageId = messageId;
                }
                context.Push(messageId, iterationCount);
            }
        }
    }
}

