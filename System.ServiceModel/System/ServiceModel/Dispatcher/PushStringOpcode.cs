namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PushStringOpcode : Opcode
    {
        private string literal;

        internal PushStringOpcode(string literal) : base(OpcodeID.PushString)
        {
            this.literal = literal;
            base.flags |= OpcodeFlags.Literal;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.literal == ((PushStringOpcode) op).literal));
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

