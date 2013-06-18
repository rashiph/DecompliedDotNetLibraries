namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Globalization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathMessageFunctionSpanStr : XPathMessageFunction
    {
        internal XPathMessageFunctionSpanStr() : base(new XPathResultType[] { XPathResultType.String }, 1, 1, XPathResultType.Number)
        {
        }

        internal static double Convert(string spanStr)
        {
            try
            {
                return TimeSpan.Parse(spanStr, CultureInfo.InvariantCulture).TotalDays;
            }
            catch (FormatException)
            {
                return double.NaN;
            }
            catch (OverflowException)
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
                string spanStr = context.PeekString(topArg.basePtr);
                context.SetValue(context, topArg.basePtr, Convert(spanStr));
                topArg.basePtr++;
            }
        }
    }
}

