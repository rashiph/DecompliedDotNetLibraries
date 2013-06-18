namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PushContextNodeOpcode : Opcode
    {
        internal PushContextNodeOpcode() : base(OpcodeID.PushContextNode)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushContextSequenceFrame();
            NodeSequence seq = context.CreateSequence();
            seq.StartNodeset();
            seq.Add(context.Processor.ContextNode);
            seq.StopNodeset();
            context.PushSequence(seq);
            return base.next;
        }
    }
}

