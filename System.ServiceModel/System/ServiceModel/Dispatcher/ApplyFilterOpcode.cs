namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class ApplyFilterOpcode : Opcode
    {
        internal ApplyFilterOpcode() : base(OpcodeID.Filter)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            StackFrame topArg = context.TopArg;
            NodeSequenceBuilder builder = new NodeSequenceBuilder(context);
            Value[] sequences = context.Sequences;
            int basePtr = topSequenceArg.basePtr;
            int index = topArg.basePtr;
            while (basePtr <= topSequenceArg.endPtr)
            {
                NodeSequence sequence = sequences[basePtr].Sequence;
                if (sequence.Count > 0)
                {
                    NodesetIterator iterator = new NodesetIterator(sequence);
                    while (iterator.NextNodeset())
                    {
                        builder.StartNodeset();
                        while (iterator.NextItem())
                        {
                            if (context.Values[index].Boolean)
                            {
                                builder.Add(ref sequence.Items[iterator.Index]);
                            }
                            index++;
                        }
                        builder.EndNodeset();
                    }
                    context.ReplaceSequenceAt(basePtr, builder.Sequence);
                    context.ReleaseSequence(sequence);
                    builder.Sequence = null;
                }
                basePtr++;
            }
            context.PopFrame();
            return base.next;
        }
    }
}

