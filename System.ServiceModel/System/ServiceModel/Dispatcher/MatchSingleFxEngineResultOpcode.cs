namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;

    internal class MatchSingleFxEngineResultOpcode : SingleFxEngineResultOpcode
    {
        internal MatchSingleFxEngineResultOpcode() : base(OpcodeID.MatchSingleFx)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            SeekableXPathNavigator contextNode = context.Processor.ContextNode;
            bool flag = this.Match(contextNode);
            context.Processor.Result = flag;
            if ((flag && (base.item != null)) && (context.Processor.MatchSet != null))
            {
                context.Processor.MatchSet.Add((MessageFilter) base.item);
            }
            return base.next;
        }

        internal bool Match(XPathNavigator nav)
        {
            object obj2 = base.Evaluate(nav);
            switch (base.xpath.ReturnType)
            {
                case XPathResultType.Number:
                    return !(((double) obj2) == 0.0);

                case XPathResultType.String:
                {
                    string str = (string) obj2;
                    return ((str != null) && (str.Length > 0));
                }
                case XPathResultType.Boolean:
                    return (bool) obj2;

                case XPathResultType.NodeSet:
                {
                    XPathNodeIterator iterator = (XPathNodeIterator) obj2;
                    return ((iterator != null) && (iterator.Count > 0));
                }
                case XPathResultType.Any:
                    return (null != obj2);
            }
            return false;
        }
    }
}

