namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class OrdinalOpcode : Opcode
    {
        internal OrdinalOpcode() : base(OpcodeID.Ordinal)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            StackFrame topArg = context.TopArg;
            Value[] sequences = context.Sequences;
            int basePtr = topSequenceArg.basePtr;
            int index = topArg.basePtr;
            while (basePtr <= topSequenceArg.endPtr)
            {
                NodeSequence sequence = sequences[basePtr].Sequence;
                for (int i = 0; i < sequence.Count; i++)
                {
                    NodeSequenceItem item = sequence[i];
                    context.Values[index].Boolean = item.Position == context.Values[index].Double;
                    index++;
                }
                basePtr++;
            }
            return base.next;
        }
    }
}

