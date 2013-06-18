namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Globalization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionDateStr : XPathMessageFunction
    {
        internal XPathMessageFunctionDateStr() : base(new XPathResultType[] { XPathResultType.String }, 1, 1, XPathResultType.Number)
        {
        }

        internal static double Convert(string dateStr)
        {
            try
            {
                return XPathMessageFunction.ConvertDate(DateTime.Parse(dateStr, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.RoundtripKind));
            }
            catch (FormatException)
            {
                return double.NaN;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return Convert(XPathMessageFunction.ToString(args[0]));
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string dateStr = context.PeekString(topArg.basePtr);
                context.SetValue(context, topArg.basePtr, Convert(dateStr));
                topArg.basePtr++;
            }
        }
    }
}

