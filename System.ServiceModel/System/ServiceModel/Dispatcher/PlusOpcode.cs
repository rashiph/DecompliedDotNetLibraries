namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PlusOpcode : MathOpcode
    {
        internal PlusOpcode() : base(OpcodeID.Plus, MathOperator.Plus)
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
                values[i].Add(values[basePtr].Double);
                basePtr++;
            }
            context.PopFrame();
            return base.next;
        }
    }
}

