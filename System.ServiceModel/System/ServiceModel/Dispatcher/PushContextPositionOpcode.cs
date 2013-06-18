namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PushContextPositionOpcode : Opcode
    {
        internal PushContextPositionOpcode() : base(OpcodeID.PushPosition)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.TransferSequencePositions();
            return base.next;
        }
    }
}

