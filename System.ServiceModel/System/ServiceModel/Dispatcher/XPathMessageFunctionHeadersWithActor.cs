namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionHeadersWithActor : XPathMessageFunction
    {
        internal XPathMessageFunctionHeadersWithActor() : base(new XPathResultType[] { XPathResultType.String }, 1, 1, XPathResultType.NodeSet)
        {
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            string str = XPathMessageFunction.ToString(args[0]);
            string xpath = string.Format(CultureInfo.InvariantCulture, "/s11:Envelope/s11:Header/*[@s11:actor='{0}'] | /s12:Envelope/s12:Header/*[@s12:role='{1}']", new object[] { str, str });
            XPathExpression expr = docContext.Compile(xpath);
            expr.SetContext((XmlNamespaceManager) xsltContext);
            return docContext.Evaluate(expr);
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame topArg = context.TopArg;
            SeekableXPathNavigator contextNode = context.Processor.ContextNode;
            long currentPosition = contextNode.CurrentPosition;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str = context.PeekString(topArg.basePtr);
                NodeSequence val = context.CreateSequence();
                if (XPathMessageFunction.MoveToHeader(contextNode) && contextNode.MoveToFirstChild())
                {
                    do
                    {
                        long num2 = contextNode.CurrentPosition;
                        string str2 = XPathMessageFunctionActor.ExtractFromNavigator(contextNode);
                        contextNode.CurrentPosition = num2;
                        if (str2 == str)
                        {
                            val.Add(contextNode);
                        }
                    }
                    while (contextNode.MoveToNext());
                }
                context.SetValue(context, topArg.basePtr, val);
                topArg.basePtr++;
            }
            contextNode.CurrentPosition = currentPosition;
        }
    }
}

