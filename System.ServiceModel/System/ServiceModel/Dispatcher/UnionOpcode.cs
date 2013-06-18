namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class UnionOpcode : Opcode
    {
        internal UnionOpcode() : base(OpcodeID.Union)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            int basePtr = topArg.basePtr;
            for (int i = secondArg.basePtr; basePtr <= topArg.endPtr; i++)
            {
                NodeSequence otherSeq = context.Values[basePtr].Sequence;
                NodeSequence sequence = context.Values[i].Sequence;
                context.SetValue(context, i, sequence.Union(context, otherSeq));
                basePtr++;
            }
            context.PopFrame();
            return base.next;
        }
    }
}

