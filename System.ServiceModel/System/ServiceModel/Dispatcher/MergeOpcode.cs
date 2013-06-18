namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class MergeOpcode : Opcode
    {
        internal MergeOpcode() : base(OpcodeID.Merge)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame topArg = context.TopArg;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                NodeSequence sequence = values[i].Sequence;
                NodeSequence val = context.CreateSequence();
                for (int j = 0; j < sequence.Count; j++)
                {
                    NodeSequenceItem item = sequence[j];
                    val.AddCopy(ref item);
                }
                val.Merge();
                context.SetValue(context, i, val);
            }
            return base.next;
        }
    }
}

