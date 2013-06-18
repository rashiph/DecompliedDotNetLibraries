namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionCorrelationData : XPathMessageFunction
    {
        private static XPathResultType[] argTypes = new XPathResultType[] { XPathResultType.String };

        public XPathMessageFunctionCorrelationData() : base(argTypes, 1, 1, XPathResultType.String)
        {
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            CorrelationDataMessageProperty property;
            string str;
            SeekableMessageNavigator navigator = docContext as SeekableMessageNavigator;
            if ((navigator != null) && (CorrelationDataMessageProperty.TryGet(navigator.Message, out property) && property.TryGetValue((string) args[0], out str)))
            {
                return str;
            }
            return string.Empty;
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame topArg = context.TopArg;
            Message contextMessage = context.Processor.ContextMessage;
            CorrelationDataMessageProperty property = null;
            CorrelationDataMessageProperty.TryGet(contextMessage, out property);
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str;
                if ((property == null) || !property.TryGetValue(context.PeekString(topArg.basePtr), out str))
                {
                    str = string.Empty;
                }
                context.SetValue(context, topArg.basePtr, str);
                topArg.basePtr++;
            }
        }
    }
}

