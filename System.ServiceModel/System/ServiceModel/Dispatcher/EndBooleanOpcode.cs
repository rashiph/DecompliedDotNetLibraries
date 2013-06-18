namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class EndBooleanOpcode : ApplyBooleanOpcode
    {
        internal EndBooleanOpcode(Opcode jump, bool test) : base(OpcodeID.EndBoolean, jump, test)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            int num = base.UpdateResultMask(context);
            context.PopFrame();
            context.PopSequenceFrame();
            if (num == 0)
            {
                return base.Jump;
            }
            return base.next;
        }
    }
}

