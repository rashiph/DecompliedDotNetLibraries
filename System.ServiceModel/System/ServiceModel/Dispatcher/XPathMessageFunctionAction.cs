namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionAction : XPathMessageFunction
    {
        public XPathMessageFunctionAction() : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        internal static string ExtractFromNavigator(XPathNavigator nav)
        {
            if (!XPathMessageFunction.MoveToAddressingHeader(nav, "Action"))
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
            string action = navigator.Message.Headers.Action;
            if (action == null)
            {
                return string.Empty;
            }
            return action;
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                string action = context.Processor.Action;
                if (action == null)
                {
                    Message contextMessage = context.Processor.ContextMessage;
                    if (contextMessage == null)
                    {
                        SeekableXPathNavigator contextNode = context.Processor.ContextNode;
                        long currentPosition = contextNode.CurrentPosition;
                        action = ExtractFromNavigator(contextNode);
                        contextNode.CurrentPosition = currentPosition;
                    }
                    else
                    {
                        action = contextMessage.Headers.Action;
                    }
                    context.Processor.Action = action;
                }
                if (action == null)
                {
                    action = string.Empty;
                    context.Processor.Action = action;
                }
                if (iterationCount == 1)
                {
                    context.Push(action);
                }
                else
                {
                    context.Push(action, iterationCount);
                }
            }
        }
    }
}

