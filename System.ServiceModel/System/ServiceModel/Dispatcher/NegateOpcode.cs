namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class NegateOpcode : MathOpcode
    {
        internal NegateOpcode() : base(OpcodeID.Negate, MathOperator.Negate)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                values[i].Negate();
            }
            return base.next;
        }
    }
}

