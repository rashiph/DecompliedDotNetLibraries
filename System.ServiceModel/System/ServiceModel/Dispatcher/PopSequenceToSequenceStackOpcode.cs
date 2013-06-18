namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PopSequenceToSequenceStackOpcode : Opcode
    {
        internal PopSequenceToSequenceStackOpcode() : base(OpcodeID.PopSequenceToSequenceStack)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushSequenceFrameFromValueStack();
            return base.next;
        }
    }
}

