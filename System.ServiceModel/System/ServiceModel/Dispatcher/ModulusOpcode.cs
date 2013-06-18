namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class ModulusOpcode : MathOpcode
    {
        internal ModulusOpcode() : base(OpcodeID.Mod, MathOperator.Mod)
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
                values[i].Double = values[basePtr].Double % values[i].Double;
                basePtr++;
            }
            context.PopFrame();
            return base.next;
        }
    }
}

