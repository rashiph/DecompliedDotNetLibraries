namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PopSequenceToValueStackOpcode : Opcode
    {
        internal PopSequenceToValueStackOpcode() : base(OpcodeID.PopSequenceToValueStack)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PopSequenceFrameToValueStack();
            return base.next;
        }
    }
}

