namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class MultiplyOpcode : MathOpcode
    {
        internal MultiplyOpcode() : base(OpcodeID.Multiply, MathOperator.Multiply)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            Value[] values = context.Values;
            int basePtr = topArg.basePtr;
            for (int i = secondArg.basePtr; basePtr <= topArg.endPtr; i++)
            {
                values[i].Multiply(values[basePtr].Double);
                basePtr++;
            }
            context.PopFrame();
            return base.next;
        }
    }
}

