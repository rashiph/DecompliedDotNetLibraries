namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PopContextNodes : Opcode
    {
        internal PopContextNodes() : base(OpcodeID.PopContextNodes)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PopContextSequenceFrame();
            return base.next;
        }
    }
}

