namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal class QuerySingleFxEngineResultOpcode : SingleFxEngineResultOpcode
    {
        internal QuerySingleFxEngineResultOpcode() : base(OpcodeID.QuerySingleFx)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            SeekableXPathNavigator contextNode = context.Processor.ContextNode;
            XPathResult result = this.Select(contextNode);
            if (context.Processor.ResultSet == null)
            {
                context.Processor.QueryResult = result;
            }
            else
            {
                context.Processor.ResultSet.Add(new KeyValuePair<MessageQuery, XPathResult>((MessageQuery) base.item, result));
            }
            return base.next;
        }

        internal XPathResult Select(XPathNavigator nav)
        {
            object obj2 = base.Evaluate(nav);
            switch (base.xpath.ReturnType)
            {
                case XPathResultType.Number:
                    return new XPathResult((double) obj2);

                case XPathResultType.String:
                    return new XPathResult((string) obj2);

                case XPathResultType.Boolean:
                    return new XPathResult((bool) obj2);

                case XPathResultType.NodeSet:
                    return new XPathResult((XPathNodeIterator) obj2);
            }
            return new XPathResult(string.Empty);
        }
    }
}

