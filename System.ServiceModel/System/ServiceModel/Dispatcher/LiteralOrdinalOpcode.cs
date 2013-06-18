namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class LiteralOrdinalOpcode : Opcode
    {
        private int ordinal;

        internal LiteralOrdinalOpcode(int ordinal) : base(OpcodeID.LiteralOrdinal)
        {
            this.ordinal = ordinal;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            Value[] sequences = context.Sequences;
            context.PushFrame();
            for (int i = topSequenceArg.basePtr; i <= topSequenceArg.endPtr; i++)
            {
                NodeSequence sequence = sequences[i].Sequence;
                for (int j = 0; j < sequence.Count; j++)
                {
                    NodeSequenceItem item = sequence[j];
                    context.Push(item.Position == this.ordinal);
                }
            }
            return base.next;
        }
    }
}

