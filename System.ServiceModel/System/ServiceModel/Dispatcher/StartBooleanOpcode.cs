namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class StartBooleanOpcode : Opcode
    {
        private bool test;

        internal StartBooleanOpcode(bool test) : base(OpcodeID.StartBoolean)
        {
            this.test = test;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (((StartBooleanOpcode) op).test == this.test));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            Value[] values = context.Values;
            StackFrame topArg = context.TopArg;
            Value[] sequences = context.Sequences;
            context.PushSequenceFrame();
            for (int i = topSequenceArg.basePtr; i <= topSequenceArg.endPtr; i++)
            {
                NodeSequence sequence = sequences[i].Sequence;
                if (sequence.Count > 0)
                {
                    NodeSequenceItem[] items = sequence.Items;
                    NodeSequence sequence2 = null;
                    int basePtr = topArg.basePtr;
                    for (int j = 0; basePtr <= topArg.endPtr; j++)
                    {
                        if (this.test == values[basePtr].Boolean)
                        {
                            if (sequence2 == null)
                            {
                                sequence2 = context.CreateSequence();
                            }
                            sequence2.AddCopy(ref items[j], NodeSequence.GetContextSize(sequence, j));
                        }
                        else if (items[j].Last && (sequence2 != null))
                        {
                            sequence2.Items[sequence2.Count - 1].Last = true;
                        }
                        basePtr++;
                    }
                    context.PushSequence((sequence2 == null) ? NodeSequence.Empty : sequence2);
                    sequence2 = null;
                }
            }
            return base.next;
        }
    }
}

