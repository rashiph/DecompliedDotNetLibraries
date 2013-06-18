namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionRelatesTo : XPathMessageFunction
    {
        private XPathExpression expr;

        internal XPathMessageFunctionRelatesTo() : base(new XPathResultType[0], 0, 0, XPathResultType.NodeSet)
        {
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (this.expr == null)
            {
                XPathExpression expression = docContext.Compile("sm:header()/wsa10:RelatesTo | sm:header()/wsaAugust2004:RelatesTo");
                expression.SetContext((XmlNamespaceManager) new XPathMessageContext());
                this.expr = expression;
            }
            return docContext.Evaluate(this.expr);
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int iterationCount = context.IterationCount;
            context.PushSequenceFrame();
            if (iterationCount > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator contextNode = context.Processor.ContextNode;
                long currentPosition = contextNode.CurrentPosition;
                if (XPathMessageFunction.MoveToAddressingHeader(contextNode, "RelatesTo"))
                {
                    seq.Add(contextNode);
                    while (XPathMessageFunction.MoveToAddressingHeaderSibling(contextNode, "RelatesTo"))
                    {
                        seq.Add(contextNode);
                    }
                }
                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < iterationCount; i++)
                {
                    seq.refCount++;
                    context.PushSequence(seq);
                }
                contextNode.CurrentPosition = currentPosition;
            }
        }
    }
}

