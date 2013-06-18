namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionDateNow : XPathMessageFunction
    {
        internal XPathMessageFunctionDateNow() : base(new XPathResultType[0], 0, 0, XPathResultType.Number)
        {
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return XPathMessageFunction.ConvertDate(DateTime.UtcNow);
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                context.Push(XPathMessageFunction.ConvertDate(DateTime.Now), iterationCount);
            }
        }
    }
}

