namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class InitialSelectOpcode : SelectOpcode
    {
        internal InitialSelectOpcode(NodeSelectCriteria criteria) : base(OpcodeID.InitialSelect, criteria, OpcodeFlags.InitialSelect)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            Value[] sequences = context.Sequences;
            bool sequenceStackInUse = context.SequenceStackInUse;
            context.PushSequenceFrame();
            for (int i = topSequenceArg.basePtr; i <= topSequenceArg.endPtr; i++)
            {
                NodeSequence sequence = sequences[i].Sequence;
                if (sequence.Count == 0)
                {
                    if (!sequenceStackInUse)
                    {
                        context.PushSequence(NodeSequence.Empty);
                    }
                }
                else
                {
                    NodeSequenceItem[] items = sequence.Items;
                    for (int j = 0; j < sequence.Count; j++)
                    {
                        SeekableXPathNavigator contextNode = items[j].GetNavigator();
                        NodeSequence destSequence = context.CreateSequence();
                        destSequence.StartNodeset();
                        base.criteria.Select(contextNode, destSequence);
                        destSequence.StopNodeset();
                        context.PushSequence(destSequence);
                    }
                }
            }
            return base.next;
        }
    }
}

