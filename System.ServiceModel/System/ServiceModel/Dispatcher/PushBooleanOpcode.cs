namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PushBooleanOpcode : Opcode
    {
        private bool literal;

        internal PushBooleanOpcode(bool literal) : base(OpcodeID.PushBool)
        {
            this.literal = literal;
            base.flags |= OpcodeFlags.Literal;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.literal == ((PushBooleanOpcode) op).literal));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                context.Push(this.literal, iterationCount);
            }
            return base.next;
        }
    }
}

