namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;

    internal class QueryResultOpcode : ResultOpcode
    {
        internal QueryResultOpcode() : base(OpcodeID.QueryResult)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            XPathResult result;
            StackFrame topArg = context.TopArg;
            switch (context.Values[topArg.basePtr].Type)
            {
                case ValueDataType.Boolean:
                    result = new XPathResult(context.Values[topArg.basePtr].GetBoolean());
                    break;

                case ValueDataType.Double:
                    result = new XPathResult(context.Values[topArg.basePtr].GetDouble());
                    break;

                case ValueDataType.Sequence:
                {
                    SafeNodeSequenceIterator nodeSetResult = new SafeNodeSequenceIterator(context.Values[topArg.basePtr].GetSequence(), context);
                    result = new XPathResult(nodeSetResult);
                    break;
                }
                case ValueDataType.String:
                    result = new XPathResult(context.Values[topArg.basePtr].GetString());
                    break;

                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }
            context.Processor.QueryResult = result;
            context.PopFrame();
            return base.next;
        }
    }
}

